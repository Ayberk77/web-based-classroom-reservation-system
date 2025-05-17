using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;

public class WeekViewModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public WeekViewModel(ApplicationDbContext context) => _context = context;

    public List<ReservationViewModel> Reservations { get; set; } = new();
    public DateTime SelectedWeek { get; set; }

    public class ReservationViewModel
    {
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public async Task OnGetAsync(DateTime? week)
    {
        SelectedWeek = week ?? StartOfWeek(DateTime.Today, DayOfWeek.Monday);
        var weekEnd = SelectedWeek.AddDays(7);

        var userEmail = User.Identity?.Name;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return;

        Reservations = await _context.Reservations
            .Include(r => r.Classroom)
            .Where(r => r.UserId == user.Id && r.StartTime >= SelectedWeek && r.StartTime < weekEnd)
            .Select(r => new ReservationViewModel
            {
                ClassroomName = r.Classroom != null ? r.Classroom.Name : "",
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status
            }).ToListAsync();
    }

    private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }
}
