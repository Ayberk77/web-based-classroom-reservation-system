using ClassroomReservationSystem.Models;

namespace ClassroomReservationSystem.Data;

public static class DataSeeder
{
    public static void SeedAdmin(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!db.Users.Any(u => u.Email == "admin@example.com"))
        {
            db.Users.Add(new User
            {
                FullName = "System Admin",
                Email = "admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "Admin"
            });
            db.SaveChanges();
        }
    }

    public static void SeedClassrooms(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.Classrooms.Any())
        {
            var classrooms = new[]
            {
                new Classroom { Name = "A101", Capacity = 30 },
                new Classroom { Name = "B202", Capacity = 50 },
                new Classroom { Name = "C303", Capacity = 40 }
            };

            context.Classrooms.AddRange(classrooms);
            context.SaveChanges();
        }
    }
}
