namespace ClassroomReservationSystem.Models;

public class Reservation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ClassroomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
    public string? Note { get; set; } // Instructor note for conflicts/holidays

    public User? User { get; set; }
    public Classroom? Classroom { get; set; }
}
