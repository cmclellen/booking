namespace Reservations.Functions;

public class ReservationEvent
{
    public string ConnectionId { get; set; }
    public string EventId { get; set; }
    public string Message { get; set; }
}