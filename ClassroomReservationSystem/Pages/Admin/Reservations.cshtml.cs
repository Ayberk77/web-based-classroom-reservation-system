using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

public class AdminReservationsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public AdminReservationsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<ReservationViewModel> Reservations { get; set; } = new();

    public async Task OnGetAsync()
    {
        var holidays = new List<DateTime> { new DateTime(2025, 1, 1), new DateTime(2025, 4, 23) }; // örnek tatiller

        var all = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Classroom)
            .Where(r => r.Status == "Pending")
            .ToListAsync();

        Reservations = all.Select(r => new ReservationViewModel
        {
            Id = r.Id,
            InstructorName = r.User?.FullName ?? "-",
            ClassroomName = r.Classroom?.Name ?? "-",
            StartTime = r.StartTime,
            EndTime = r.EndTime,
            Status = r.Status,
            IsHoliday = holidays.Contains(r.StartTime.Date),
            IsConflicting = all.Any(o => o.Id != r.Id && o.ClassroomId == r.ClassroomId &&
                r.StartTime < o.EndTime && o.StartTime < r.EndTime)
        }).ToList();
    }

    public async Task<IActionResult> OnPostApproveAsync(int id)
    {
        var r = await _context.Reservations.FindAsync(id);
        if (r != null)
        {
            r.Status = "Approved";
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRejectAsync(int id)
    {
        var r = await _context.Reservations.FindAsync(id);
        if (r != null)
        {
            r.Status = "Rejected";
            await _context.SaveChangesAsync();
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
    }
}