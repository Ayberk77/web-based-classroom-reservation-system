using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;

public class MyReservationsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public MyReservationsModel(ApplicationDbContext context) => _context = context;

    public List<ReservationViewModel> Reservations { get; set; } = new();

    public class ReservationViewModel
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
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
            .OrderByDescending(r => r.StartTime)
            .Select(r => new ReservationViewModel
            {
                Id = r.Id,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                ClassroomName = r.Classroom != null ? r.Classroom.Name : "",
                Status = r.Status
            })
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation != null && reservation.Status == "Pending")
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Reservation canceled.";
        }
        return RedirectToPage();
    }
}
