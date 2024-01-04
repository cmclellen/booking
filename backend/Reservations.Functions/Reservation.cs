using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Reservations.Functions.Repositories;

namespace Reservations.Functions
{
    public class Reservation
    {
        private readonly ILogger<Reservation> _logger;
        private readonly IReservationEventRepository _reservationEventRepository;

        public Reservation(
            ILogger<Reservation> logger,
            IReservationEventRepository reservationEventRepository)
        {
            _logger = logger;
            _reservationEventRepository = reservationEventRepository;
        }

        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = "serverless")]
            SignalRConnectionInfo connectionInfo)
        {
            //return Negotiate(req.Headers["x-ms-signalr-user-id"]);
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
                    throw;
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
                    throw;
                }
            }
            catch (Exception err)
            {
                logger.LogError(err, "Failed orchestrating reservations.");
                throw;
            }

            logger.LogInformation("Orchestration completed successfully.");
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

        private async Task CancelReservation(string type, ReservationRequest reservationRequest,
            IAsyncCollector<SignalRMessage> signalRMessages, CancellationToken cancellationToken)
        {
            await SendMessageAsync(reservationRequest, type, signalRMessages, $"Cancelling {type} reservation...",
                cancellationToken);
            await SimulateProcessRequest(type, reservationRequest, signalRMessages, false, cancellationToken);
            await SendMessageAsync(reservationRequest, type, signalRMessages, $"{type} reservation cancelled.",
                cancellationToken);
        }

        [FunctionName(nameof(OnConnected))]
        public async Task OnConnected([SignalRTrigger("serverless", "connections", "connected")] InvocationContext invocationContext, ILogger logger)
        {
            logger.LogInformation($"{invocationContext.ConnectionId} has connected");
            await Task.CompletedTask;
        }

        [FunctionName(nameof(OnDisconnected))]
        public async Task OnDisconnected([SignalRTrigger("serverless", "connections", "disconnected")] InvocationContext invocationContext)
        {
            await Task.CompletedTask;
        }

        [FunctionName(nameof(ReservationEventAck))]
        public async Task ReservationEventAck([SignalRTrigger("serverless", "messages", "ReservationEventAck", parameterNames: new string[] { "message" })] InvocationContext invocationContext, string message)
        {
            _logger.LogInformation($"ReservationEventAck \"{message}\" from {invocationContext.ConnectionId}.");
            await Task.CompletedTask;
        }

        [FunctionName(nameof(ReserveCar))]
        public async Task ReserveCar([ActivityTrigger] ReservationRequest reservationRequest, ILogger log,
            [SignalR(HubName = "serverless")] IAsyncCollector<SignalRMessage> signalRMessages,
            CancellationToken cancellationToken)
        {
            var type = "Car";
            await MakeReservation(type, reservationRequest, signalRMessages, cancellationToken);
        }

        private async Task MakeReservation(string type, ReservationRequest reservationRequest,
            IAsyncCollector<SignalRMessage> signalRMessages, CancellationToken cancellationToken)
        {
            await SendMessageAsync(reservationRequest, type, signalRMessages, $"Reserving {type}...",
                cancellationToken);
            await SimulateProcessRequest(type, reservationRequest, signalRMessages, true, cancellationToken);
            await SendMessageAsync(reservationRequest, type, signalRMessages, $"{type} reserved.", cancellationToken);
        }

        private async Task SendMessageAsync(ReservationRequest reservationRequest, string type,
            IAsyncCollector<SignalRMessage> signalRMessages,
            string message,
            CancellationToken cancellationToken)
        {
            var eventId = await _reservationEventRepository.AddAsync(reservationRequest.ConnectionId, reservationRequest.Id, message,
                cancellationToken);
            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = reservationRequest.ConnectionId,
                Target = "ReservationEvent",
                Arguments = new object[] { message, type, reservationRequest.Id, eventId.ToString("D") }
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
            var type = "Flight";
            await MakeReservation(type, reservationRequest, signalRMessages, cancellationToken);
        }

        private async Task SimulateProcessRequest(string type, ReservationRequest reservationRequest,
            IAsyncCollector<SignalRMessage> signalRMessages, bool canFail,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
            if (canFail && reservationRequest.SimulateFailure == type)
            {
                await SendMessageAsync(reservationRequest, type, signalRMessages, $"Error occurred reserving {type}.",
                    cancellationToken);
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