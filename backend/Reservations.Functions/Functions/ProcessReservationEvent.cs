using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Reservations.Functions.Repositories;
using Reservations.Functions.Utils;

namespace Reservations.Functions.Functions
{
    public class ProcessReservationEvent
    {
        private readonly ILogger<ProcessReservationEvent> _logger;
        private readonly IReservationEventRepository _reservationEventRepository;

        public ProcessReservationEvent(
            ILogger<ProcessReservationEvent> logger,
            IReservationEventRepository reservationEventRepository)
        {
            _logger = logger;
            _reservationEventRepository = reservationEventRepository;
        }

        [FunctionName(nameof(ProcessReservationEvent))]
        public async Task Run(
            [QueueTrigger("reservation-events")] ReservationEvent reservationEvent, 
            [SignalR(HubName = Constants.SignalRHubName)] IAsyncCollector<SignalRMessage> signalRMessages, CancellationToken cancellationToken)
        {
            Guard.Against.Null(reservationEvent, nameof(reservationEvent));

            _logger.LogDebug("Adding event to table...");
            var eventId = await _reservationEventRepository.AddAsync(reservationEvent.ConnectionId,
                reservationEvent.InvocationId, reservationEvent.Message,
                cancellationToken);
            _logger.LogInformation("Successfully added event to table.");

            _logger.LogDebug("Sending SignalR messages...");
            await signalRMessages.AddAsync(new SignalRMessage
            {
                ConnectionId = reservationEvent.ConnectionId,
                Target = "ReservationEvent",
                Arguments = new object[] { reservationEvent.Message, reservationEvent.Type, reservationEvent.InvocationId, eventId.ToString("D") }
            }, cancellationToken);

            _logger.LogInformation("Successfully sent SignalR messages.");

            await Task.CompletedTask;
        }
    }
}