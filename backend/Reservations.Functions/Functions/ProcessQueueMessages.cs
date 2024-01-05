using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Reservations.Functions.Events;
using Reservations.Functions.Repositories;
using Reservations.Functions.Utils;

namespace Reservations.Functions.Functions
{
    public class ProcessQueueMessages
    {
        private readonly ILogger<ProcessQueueMessages> _logger;
        private readonly IReservationEventRepository _reservationEventRepository;

        public ProcessQueueMessages(
            ILogger<ProcessQueueMessages> logger,
            IReservationEventRepository reservationEventRepository)
        {
            _logger = logger;
            _reservationEventRepository = reservationEventRepository;
        }

        [FunctionName(nameof(ProcessReservationEvents))]
        public async Task ProcessReservationEvents(
            [QueueTrigger("reservation-events")] ReservationEvent reservationEvent, DateTimeOffset insertionTime,
            [SignalR(HubName = Constants.SignalRHubName)]
            IAsyncCollector<SignalRMessage> signalRMessages, CancellationToken cancellationToken)
        {
            Guard.Against.Null(reservationEvent, nameof(reservationEvent));

            _logger.LogDebug("Adding event to table...");
            var eventId = await _reservationEventRepository.AddAsync(reservationEvent.ConnectionId,
                reservationEvent.InvocationId, reservationEvent.Message, insertionTime.UtcDateTime, cancellationToken);
            _logger.LogInformation("Successfully added event to table.");

            _logger.LogDebug("Sending SignalR message...");
            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = reservationEvent.ConnectionId,
                Target = "ReservationEvent",
                Arguments = new object[]
                {
                    reservationEvent.Message, reservationEvent.Type, reservationEvent.InvocationId,
                    eventId.ToString("D"), insertionTime.UtcDateTime
                }
            }, cancellationToken);
            _logger.LogInformation("Successfully sent SignalR message.");
        }

        [FunctionName(nameof(ProcessReservationAckEvents))]
        public async Task ProcessReservationAckEvents(
            [QueueTrigger("reservation-ack-events")]
            ReservationAckEvent reservationAckEvent,
            CancellationToken cancellationToken)
        {
            Guard.Against.Null(reservationAckEvent, nameof(reservationAckEvent));

            _logger.LogDebug("Updating event to acknowledged {InvocationId} {EventId}...",
                reservationAckEvent.InvocationId, reservationAckEvent.EventId);
            await _reservationEventRepository.AcknowledgeAsync(reservationAckEvent.ConnectionId,
                reservationAckEvent.InvocationId, reservationAckEvent.EventId, cancellationToken);
            _logger.LogInformation("Successfully updated event to acknowledged {InvocationId} {EventId}.",
                reservationAckEvent.InvocationId, reservationAckEvent.EventId);
        }
    }
}