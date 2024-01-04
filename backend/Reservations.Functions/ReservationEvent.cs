namespace Reservations.Functions;

public class ReservationEvent
{
    public string ConnectionId { get; set; }
    public string InvocationId { get; set; }
    public string Message { get; set; }
    public string Type { get; set; }
}