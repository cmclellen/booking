namespace Reservations.Functions;

public class ReservationRequest
{
    public string Id { get; set; }
    public string ConnectionId { get; set; }
    public string SimulateFailure { get; set; }
}