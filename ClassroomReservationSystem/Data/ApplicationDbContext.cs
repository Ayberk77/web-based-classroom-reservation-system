using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Models;

namespace ClassroomReservationSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Term> Terms { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Classroom> Classrooms { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<SystemLog> SystemLogs { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }

}


