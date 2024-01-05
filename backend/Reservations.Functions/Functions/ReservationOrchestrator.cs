using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Reservations.Functions.Events;
using Reservations.Functions.Utils;

namespace Reservations.Functions.Functions
{
    public class ReservationOrchestrator
    {
        private readonly ILogger<ReservationOrchestrator> _logger;
        private readonly IEventPublisher _eventPublisher;

        public ReservationOrchestrator(
            ILogger<ReservationOrchestrator> logger,
            IEventPublisher eventPublisher)
        {
            _logger = logger;
            _eventPublisher = eventPublisher;
        }

        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate(
            [HttpTrigger(AuthorizationLevel.Anonymous)]
            HttpRequest req,
            [SignalRConnectionInfo(HubName = Constants.SignalRHubName)]
            SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        [FunctionName(nameof(ReservationOrchestrator))]
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
        public async Task CancelCarReservation([ActivityTrigger] ReservationRequest reservationRequest,
            CancellationToken cancellationToken)
        {
            var type = "Car";
            await CancelReservation(type, reservationRequest, cancellationToken);
        }

        [FunctionName(nameof(CancelFlightReservation))]
        public async Task CancelFlightReservation([ActivityTrigger] ReservationRequest reservationRequest,
            CancellationToken cancellationToken)
        {
            var type = "Flight";
            await CancelReservation(type, reservationRequest, cancellationToken);
        }

        private async Task CancelReservation(string type, ReservationRequest reservationRequest,
            CancellationToken cancellationToken)
        {
            await SendMessageAsync(reservationRequest, type, $"Cancelling {type} reservation...",
                cancellationToken);
            await SimulateProcessRequest(type, reservationRequest, false, cancellationToken);
            await SendMessageAsync(reservationRequest, type, $"{type} reservation cancelled.",
                cancellationToken);
        }

        [FunctionName(nameof(OnConnected))]
        public async Task OnConnected(
            [SignalRTrigger(Constants.SignalRHubName, "connections", "connected")]
            InvocationContext invocationContext,
            ILogger logger)
        {
            logger.LogInformation($"{invocationContext.ConnectionId} has connected");
            await Task.CompletedTask;
        }

        [FunctionName(nameof(OnDisconnected))]
        public async Task OnDisconnected(
            [SignalRTrigger(Constants.SignalRHubName, "connections", "disconnected")]
            InvocationContext invocationContext)
        {
            await Task.CompletedTask;
        }

        [FunctionName(nameof(ReservationEventAck))]
        public async Task ReservationEventAck(
            [SignalRTrigger(Constants.SignalRHubName, "messages", "ReservationEventAck", "invocationId", "eventId")]
            InvocationContext invocationContext, string invocationId, string eventId,
            CancellationToken cancellationToken)
        {
            var @event = new ReservationAckEvent
            {
                ConnectionId = invocationContext.ConnectionId,
                InvocationId = invocationId,
                EventId = eventId
            };
            await _eventPublisher.PublishAsync(@event, cancellationToken);
        }

        [FunctionName(nameof(ReserveCar))]
        public async Task ReserveCar([ActivityTrigger] ReservationRequest reservationRequest,
            CancellationToken cancellationToken)
        {
            var type = "Car";
            await MakeReservation(type, reservationRequest, cancellationToken);
        }

        private async Task MakeReservation(string type, ReservationRequest reservationRequest,
            CancellationToken cancellationToken)
        {
            await SendMessageAsync(reservationRequest, type, $"Reserving {type}...",
                cancellationToken);
            await SimulateProcessRequest(type, reservationRequest, true, cancellationToken);
            await SendMessageAsync(reservationRequest, type, $"{type} reserved.", cancellationToken);
        }

        private async Task SendMessageAsync(ReservationRequest reservationRequest, string type, string message,
            CancellationToken cancellationToken)
        {
            var @event = new ReservationEvent
            {
                ConnectionId = reservationRequest.ConnectionId,
                InvocationId = reservationRequest.Id,
                Message = message,
                Type = type
            };
            await _eventPublisher.PublishAsync(@event, cancellationToken);
        }

        [FunctionName(nameof(ReserveHotel))]
        public async Task ReserveHotel([ActivityTrigger] ReservationRequest reservationRequest,
            CancellationToken cancellationToken)
        {
            var type = "Hotel";
            await MakeReservation(type, reservationRequest, cancellationToken);
        }

        [FunctionName(nameof(ReserveFlight))]
        public async Task ReserveFlight([ActivityTrigger] ReservationRequest reservationRequest, ILogger log,
            CancellationToken cancellationToken)
        {
            var type = "Flight";
            await MakeReservation(type, reservationRequest, cancellationToken);
        }

        private async Task SimulateProcessRequest(string type, ReservationRequest reservationRequest,
            bool canFail,
            CancellationToken cancellationToken = default)
        {
            await Task.Delay(1000, cancellationToken);
            if (canFail && reservationRequest.SimulateFailure == type)
            {
                await SendMessageAsync(reservationRequest, type, $"Error occurred reserving {type}.",
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
            var instanceId = await starter.StartNewAsync(nameof(ReservationOrchestrator), makeReservationRequest);
            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);
            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}