using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

public class ReservationRequestModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ReservationRequestModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public List<SelectListItem> ClassroomOptions { get; set; } = new();
    public string WarningMessage { get; set; } = string.Empty;

    public class InputModel
    {
        public int ClassroomId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public async Task<IActionResult> OnGetAsync()
    {
        ClassroomOptions = await _context.Classrooms
            .Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() })
            .ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.StartTime >= Input.EndTime)
        {
            WarningMessage = "Start time must be before end time.";
            await OnGetAsync();
            return Page();
        }

        var userEmail = User.Identity?.Name;
        var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (instructor == null) return NotFound();

        var term = await _context.Terms
            .Where(t => t.StartDate <= DateTime.Now && t.EndDate >= DateTime.Now)
            .OrderByDescending(t => t.StartDate)
            .FirstOrDefaultAsync();

        if (term == null)
        {
            WarningMessage = "No active term defined by admin.";
            await OnGetAsync();
            return Page();
        }

        var holidays = new List<DateTime> {
            new DateTime(2025, 1, 1), new DateTime(2025, 4, 23),
            new DateTime(2025, 5, 1), new DateTime(2025, 5, 19)
        };

        var reservations = new List<Reservation>();

        for (var date = term.StartDate.Date; date <= term.EndDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != Input.DayOfWeek) continue;

            if (holidays.Contains(date))
            {
                WarningMessage = $"{date:yyyy-MM-dd} is a holiday. Cannot reserve.";
                await OnGetAsync();
                return Page();
            }

            var startDateTime = date + Input.StartTime;
            var endDateTime = date + Input.EndTime;

            var conflict = await _context.Reservations.AnyAsync(r =>
                r.ClassroomId == Input.ClassroomId &&
                r.Status == "Approved" &&
                ((startDateTime < r.EndTime) && (r.StartTime < endDateTime)));

            if (conflict)
            {
                WarningMessage = $"Conflict on {date:yyyy-MM-dd}. Request aborted.";
                await OnGetAsync();
                return Page();
            }

            reservations.Add(new Reservation
            {
                ClassroomId = Input.ClassroomId,
                UserId = instructor.Id,
                StartTime = startDateTime,
                EndTime = endDateTime,
                Status = "Pending"
            });
        }

        _context.Reservations.AddRange(reservations);
        await _context.SaveChangesAsync();

        return RedirectToPage("/Instructor/Index");
    }
}