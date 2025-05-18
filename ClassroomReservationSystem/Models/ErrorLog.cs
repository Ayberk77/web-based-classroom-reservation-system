namespace ClassroomReservationSystem.Models;

public class ErrorLog
{
    public int Id { get; set; }

    public string ExceptionMessage { get; set; } = string.Empty;

    public string StackTrace { get; set; } = string.Empty;

    public DateTime Time { get; set; }

    public string? UserContext { get; set; }

    public string? RequestPath { get; set; }

    public string? QueryString { get; set; }

    public string? UserAgent { get; set; }

    public string? IPAddress { get; set; }

    public string? UserEmail { get; set; }
}
