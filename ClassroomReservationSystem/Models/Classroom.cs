namespace ClassroomReservationSystem.Models;

public class Classroom
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
}