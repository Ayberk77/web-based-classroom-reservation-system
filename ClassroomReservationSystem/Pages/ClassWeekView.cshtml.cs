using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;

public class ClassWeekViewModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ClassWeekViewModel(ApplicationDbContext context) => _context = context;

    public List<ReservationViewModel> Reservations { get; set; } = new();
    public DateTime SelectedWeek { get; set; }
    public int SelectedClassroomId { get; set; }

    public class ReservationViewModel
    {
        public string InstructorName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public async Task OnGetAsync(DateTime? week, int classroomId)
    {
        SelectedWeek = week ?? StartOfWeek(DateTime.Today, DayOfWeek.Monday);
        var weekEnd = SelectedWeek.AddDays(7);
        SelectedClassroomId = classroomId;

        Reservations = await _context.Reservations
            .Include(r => r.User)
            .Where(r => r.ClassroomId == classroomId && r.StartTime >= SelectedWeek && r.StartTime < weekEnd)
            .Select(r => new ReservationViewModel
            {
                InstructorName = r.User != null ? r.User.FullName : "",
                StartTime = r.StartTime,
                EndTime = r.EndTime
            }).ToListAsync();
    }

    private static DateTime StartOfWeek(DateTime date, DayOfWeek startOfWeek)
    {
        int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
        return date.AddDays(-diff).Date;
    }
}
