using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;

namespace Reservations.Functions
{
    public class Reservation
    {
        private readonly ILogger<Reservation> _logger;

        public Reservation(ILogger<Reservation> logger)
        {
            _logger = logger;
        }

        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = "serverless")]
            SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName(nameof(Reservation))]
        public async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var logger = context.CreateReplaySafeLogger(_logger);
            try
            {
                logger.LogDebug("Initiating orchestration...");
                var makeReservationRequest = context.GetInput<ReservationRequest>();

                logger.LogInformation("Reserving flight...");
                await context.CallActivityAsync(nameof(ReserveFlight), makeReservationRequest);
                logger.LogInformation("Successfully reserved flight.");

                try
                {
                    logger.LogInformation("Reserving car rental...");
                    await context.CallActivityAsync(nameof(ReserveCar), makeReservationRequest);
                    logger.LogInformation("Successfully reserved car rental.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to reserve car rental.");

                    logger.LogInformation("Cancelling flight...");
                    await context.CallActivityAsync(nameof(CancelFlightReservation), makeReservationRequest);
                    logger.LogInformation("Successfully cancelled flight.");
                    return;
                }

                try
                {
                    logger.LogInformation("Reserving hotel...");
                    await context.CallActivityAsync(nameof(ReserveHotel), makeReservationRequest);
                    logger.LogInformation("Successfully reserved hotel.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to reserve hotel.");

                    logger.LogInformation("Cancelling car rental...");
                    await context.CallActivityAsync(nameof(CancelCarReservation), makeReservationRequest);
                    logger.LogInformation("Successfully cancelled car rental.");

                    logger.LogInformation("Cancelling flight...");
                    await context.CallActivityAsync(nameof(CancelFlightReservation), makeReservationRequest);
                    logger.LogInformation("Successfully cancelled flight.");
                }
            } catch(Exception err)
            {
                logger.LogError(err, "Failed orchestrating reservations.");
            }
        }

        [FunctionName(nameof(CancelCarReservation))]
        public async Task CancelCarReservation([ActivityTrigger] ReservationRequest reservationRequest, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            var type = "Car";
            await CancelReservation(type, reservationRequest, signalRMessages, cancellationToken);
        }

        [FunctionName(nameof(CancelFlightReservation))]
        public async Task CancelFlightReservation([ActivityTrigger] ReservationRequest reservationRequest, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            var type = "Flight";
            await CancelReservation(type, reservationRequest, signalRMessages, cancellationToken);
        }

        private async Task CancelReservation(string type, ReservationRequest reservationRequest, IAsyncCollector<SignalRMessage> signalRMessages, CancellationToken cancellationToken)
        {
            var connectionId = reservationRequest.ConnectionId;
            await SendMessageAsync(connectionId, signalRMessages, $"Cancelling {type} reservation...", cancellationToken);
            await SimulateProcessRequest(type, reservationRequest, signalRMessages, false, cancellationToken);
            await SendMessageAsync(connectionId, signalRMessages, $"{type} reservation cancelled.", cancellationToken);
        }

        [FunctionName(nameof(ReserveCar))]
        public async Task ReserveCar([ActivityTrigger] ReservationRequest reservationRequest, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            var type = "Car";
            await MakeReservation(type, reservationRequest, signalRMessages, cancellationToken);
        }

        private async Task MakeReservation(string type, ReservationRequest reservationRequest, IAsyncCollector<SignalRMessage> signalRMessages, CancellationToken cancellationToken)
        {
            var connectionId = reservationRequest.ConnectionId;
            await SendMessageAsync(connectionId, signalRMessages, $"Reserving {type}...", cancellationToken);
            await SimulateProcessRequest(type, reservationRequest, signalRMessages, true, cancellationToken);
            await SendMessageAsync(connectionId, signalRMessages, $"{type} reserved.", cancellationToken);
        }

        private async Task SendMessageAsync(string connectionId,
            IAsyncCollector<SignalRMessage> signalRMessages,
            string message,
            CancellationToken cancellationToken)
        {
            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = connectionId,
                Target = "ReservationEvent",
                Arguments = new[] { message }
            }, cancellationToken);
        }

        [FunctionName(nameof(ReserveHotel))]
        public async Task ReserveHotel([ActivityTrigger] ReservationRequest reservationRequest, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            var type = "Hotel";
            await MakeReservation(type, reservationRequest, signalRMessages, cancellationToken);
        }

        [FunctionName(nameof(ReserveFlight))]
        public async Task ReserveFlight([ActivityTrigger] ReservationRequest reservationRequest, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            var type = "Flight"; await MakeReservation(type, reservationRequest, signalRMessages, cancellationToken);
        }

        private async Task SimulateProcessRequest(string type, ReservationRequest reservationRequest, IAsyncCollector<SignalRMessage> signalRMessages, bool canFail,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
            if (canFail && reservationRequest.SimulateFailure == type)
            {
                var connectionId = reservationRequest.ConnectionId;
                await SendMessageAsync(connectionId, signalRMessages, $"Error occurred reserving {type}.", cancellationToken);
                throw new Exception("Simulated error");
            }
        }

        [FunctionName("Reservation_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            var makeReservationRequest = await req.Content!.ReadFromJsonAsync<ReservationRequest>();
            var instanceId = await starter.StartNewAsync(nameof(Reservation), makeReservationRequest);
            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}