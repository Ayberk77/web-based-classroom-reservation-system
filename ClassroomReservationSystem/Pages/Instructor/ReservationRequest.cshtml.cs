using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using System.Security.Claims;

public class ReservationRequestModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly HolidayService _holidayService;
    private readonly IEmailSender _emailSender;
    private readonly ILogService _logService;

    public ReservationRequestModel(ApplicationDbContext context, HolidayService holidayService, IEmailSender emailSender, ILogService logService)
    {
        _context = context;
        _holidayService = holidayService;
        _emailSender = emailSender;
        _logService = logService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public List<SelectListItem> ClassroomOptions { get; set; } = new();
    public string WarningMessage { get; set; } = string.Empty;

    [BindProperty]
    public bool ConfirmAnyway { get; set; } = false;
    [BindProperty]
    public string InstructorNote { get; set; } = string.Empty;

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

        var reservations = new List<Reservation>();
        bool hasConflict = false, hasHoliday = false;

        for (var date = term.StartDate.Date; date <= term.EndDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != Input.DayOfWeek) continue;

            var startDateTime = date + Input.StartTime;
            var endDateTime = date + Input.EndTime;

            bool isHoliday = await _holidayService.IsHolidayAsync(date);
            bool isConflict = await _context.Reservations.AnyAsync(r =>
                r.ClassroomId == Input.ClassroomId &&
                r.Status == "Approved" &&
                ((startDateTime < r.EndTime) && (r.StartTime < endDateTime)));

            if (isHoliday) hasHoliday = true;
            if (isConflict) hasConflict = true;
        }

        if ((hasHoliday || hasConflict) && !ConfirmAnyway)
        {
            WarningMessage = "Your reservation conflicts with another or falls on a holiday. Confirm to proceed.";
            await OnGetAsync();
            return Page();
        }

        for (var date = term.StartDate.Date; date <= term.EndDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek != Input.DayOfWeek) continue;

            var startDateTime = date + Input.StartTime;
            var endDateTime = date + Input.EndTime;

            bool isHoliday = await _holidayService.IsHolidayAsync(date);
            bool isConflict = await _context.Reservations.AnyAsync(r =>
                r.ClassroomId == Input.ClassroomId &&
                r.Status == "Approved" &&
                ((startDateTime < r.EndTime) && (r.StartTime < endDateTime)));

            reservations.Add(new Reservation
            {
                ClassroomId = Input.ClassroomId,
                UserId = instructor.Id,
                StartTime = startDateTime,
                EndTime = endDateTime,
                Status = "Pending",
                Note = (isHoliday || isConflict) ? InstructorNote : null
            });
        }

        _context.Reservations.AddRange(reservations);
        await _context.SaveChangesAsync();

        var classroom = await _context.Classrooms.FindAsync(Input.ClassroomId);
        await _logService.LogActionAsync(
            instructor.Id,
            $"Submitted reservation request for classroom: {classroom?.Name ?? "Unknown"} ({Input.DayOfWeek} {Input.StartTime}-{Input.EndTime})",
            "Success"
        );

        if ((hasHoliday || hasConflict) && !string.IsNullOrWhiteSpace(instructor.Email))
        {
            await _emailSender.SendEmailAsync(instructor.Email, "Reservation Warning",
                "Your reservation includes dates that fall on a public holiday or conflict with existing bookings. Please ensure you intended this.");
        }

        return RedirectToPage("/Instructor/Index");
    }
}
