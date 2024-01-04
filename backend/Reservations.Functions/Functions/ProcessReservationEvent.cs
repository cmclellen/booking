using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace Reservations.Functions.Functions
{
    public class ProcessReservationEvent
    {
        private readonly ILogger<ProcessReservationEvent> _logger;

        public ProcessReservationEvent(ILogger<ProcessReservationEvent> logger)
        {
            _logger = logger;
        }

        [FunctionName(nameof(ProcessReservationEvent))]
        public async Task Run(
            [QueueTrigger("reservation-events")] ReservationEvent reservationEvent)
        {
            _logger.LogInformation($"C# function processed queue item: {reservationEvent.Message}");
            await Task.CompletedTask;
        }
    }
}