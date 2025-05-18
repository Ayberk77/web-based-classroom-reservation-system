using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

public class AdminReservationsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly HolidayService _holidayService;
    private readonly ILogService _logService;

    public AdminReservationsModel(ApplicationDbContext context, IEmailSender emailSender, HolidayService holidayService, ILogService logService)
    {
        _context = context;
        _emailSender = emailSender;
        _holidayService = holidayService;
        _logService = logService;
    }

    public List<ReservationViewModel> Reservations { get; set; } = new();

    private int? GetCurrentUserId()
    {
        var claim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
    }

    public async Task OnGetAsync()
    {
        var all = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Classroom)
            .ToListAsync();

        if (!all.Any()) {
            Reservations = new List<ReservationViewModel>();
            return;
        }

        var minDate = all.Min(r => r.StartTime).Date;
        var maxDate = all.Max(r => r.StartTime).Date;
        var holidaySet = await _holidayService.GetHolidaySetAsync(minDate, maxDate);

        Reservations = all.Select(r =>
        {
            bool isHoliday = holidaySet.Contains(r.StartTime.Date);
            bool isConflicting = all.Any(o => o.Id != r.Id && o.ClassroomId == r.ClassroomId && o.Status == "Approved" &&
                r.StartTime < o.EndTime && o.StartTime < r.EndTime);

            return new ReservationViewModel
            {
                Id = r.Id,
                InstructorName = r.User?.FullName ?? "-",
                ClassroomName = r.Classroom?.Name ?? "-",
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                IsHoliday = isHoliday,
                IsConflicting = isConflicting,
                Note = (isHoliday || isConflicting) ? (r.Note?.Length > 100 ? r.Note.Substring(0, 100) + "..." : r.Note) : null
            };
        }).ToList();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var r = await _context.Reservations
            .Include(r => r.Classroom)
            .Include(r => r.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return NotFound();

        bool isHoliday = await _holidayService.IsHolidayAsync(r.StartTime.Date);

        bool isConflicting = await _context.Reservations
            .Where(o => o.Id != r.Id && o.ClassroomId == r.ClassroomId && o.Status == "Approved")
            .AnyAsync(o => r.StartTime < o.EndTime && o.StartTime < r.EndTime);

        if (isHoliday || isConflicting)
        {
            TempData["Error"] = "Reservation falls on a holiday or conflicts with another. Edit it first.";
            return RedirectToPage();
        }

        r.Status = "Approved";

        var all = await _context.Reservations.ToListAsync();
        foreach (var other in all)
        {
            if (other.Id == r.Id) continue;
            if (other.ClassroomId == r.ClassroomId && other.Status == "Approved")
            {
                bool isNowConflicting = r.StartTime < other.EndTime && other.StartTime < r.EndTime;
                if (isNowConflicting)
                {
                    other.Status = "Pending";
                }
            }
        }

        await _context.SaveChangesAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(r.User?.Email))
                {
                    await _emailSender.SendEmailAsync(
                        r.User.Email,
                        "Reservation Approved",
                        $"Your reservation on {r.StartTime:g} to {r.EndTime:g} for classroom {r.Classroom?.Name} has been approved."
                    );
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(ex, r.User?.Email);
            }
        });

        await _logService.LogActionAsync(GetCurrentUserId(), $"Approved reservation ID {r.Id}", "Success");

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var r = await _context.Reservations
            .Include(r => r.Classroom)
            .Include(r => r.User)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r != null)
        {
            r.Status = "Rejected";
            await _context.SaveChangesAsync();

            try
            {
                if (!string.IsNullOrWhiteSpace(r.User?.Email))
                {
                    await _emailSender.SendEmailAsync(r.User.Email, "Reservation Rejected",
                        $"Your reservation on {r.StartTime:g} to {r.EndTime:g} for classroom {r.Classroom?.Name} has been rejected.");
                }
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync(ex, r.User?.Email);
            }

            await _logService.LogActionAsync(GetCurrentUserId(), $"Rejected reservation ID {r.Id}", "Success");
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var r = await _context.Reservations.FindAsync(id);
        if (r != null)
        {
            _context.Reservations.Remove(r);
            await _context.SaveChangesAsync();
            await _logService.LogActionAsync(GetCurrentUserId(), $"Deleted reservation ID {id}", "Success");
        }
        return RedirectToPage();
    }

    public class ReservationViewModel
    {
        public int Id { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsConflicting { get; set; }
        public bool IsHoliday { get; set; }
        public string? Note { get; set; }
    }
}
