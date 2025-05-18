using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public class MyReservationsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogService _logService;

    public MyReservationsModel(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public List<ReservationViewModel> Reservations { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userEmail = User.Identity?.Name;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null) return;

        Reservations = await _context.Reservations
            .Where(r => r.UserId == user.Id)
            .Include(r => r.Classroom)
            .Select(r => new ReservationViewModel
            {
                Id = r.Id,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                ClassroomName = r.Classroom != null ? r.Classroom.Name : "-",
                Status = r.Status
            }).ToListAsync();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        await _logService.LogActionAsync(reservation.UserId, $"Cancelled reservation ID {id}", "Success");

        TempData["Success"] = "Reservation cancelled.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteRejectedAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null || reservation.Status != "Rejected")
        {
            TempData["Success"] = "Invalid delete attempt.";
            return RedirectToPage();
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        await _logService.LogActionAsync(reservation.UserId, $"Deleted rejected reservation ID {id}", "Success");

        TempData["Success"] = "Rejected reservation deleted.";
        return RedirectToPage();
    }

    public class ReservationViewModel
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
