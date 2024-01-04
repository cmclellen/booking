namespace Reservations.Functions.Events
{
    public class ReservationAckEvent
    {
        public string ConnectionId { get; set; }
        public string InvocationId { get; set; }
        public string EventId { get; set; }
    }
}