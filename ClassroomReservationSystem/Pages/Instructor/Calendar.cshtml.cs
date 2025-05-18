using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;

public class CalendarModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CalendarModel(ApplicationDbContext context) => _context = context;

    public List<ReservationViewModel> Reservations { get; set; } = new();

    public string CurrentWeek { get; set; } = "";


    public class ReservationViewModel
    {
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        var userEmail = User.Identity?.Name;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return;

        Reservations = await _context.Reservations
            .Include(r => r.Classroom)
            .Where(r => r.UserId == user.Id)
            .Select(r => new ReservationViewModel
            {
                ClassroomName = r.Classroom != null ? r.Classroom.Name : "",
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status
            }).ToListAsync();
        
        Console.WriteLine("[DEBUG] Reservation count = " + Reservations.Count);
        foreach (var r in Reservations)
        {
            Console.WriteLine($"[DEBUG] {r.ClassroomName} {r.Status} {r.StartTime} - {r.EndTime}");
        }

    }
}
