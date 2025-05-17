namespace ClassroomReservationSystem.Models;
public class Feedback
{
    public int Id { get; set; }

    public int ClassroomId { get; set; }       
    public Classroom Classroom { get; set; } = default!;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int Rating { get; set; } // 1 to 5
    public string? Comment { get; set; }
}
