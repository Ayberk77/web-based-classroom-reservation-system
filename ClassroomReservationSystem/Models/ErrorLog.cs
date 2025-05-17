namespace ClassroomReservationSystem.Models;

public class ErrorLog
{
    public int Id { get; set; }
    public string Exception { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string? UserContext { get; set; }
}