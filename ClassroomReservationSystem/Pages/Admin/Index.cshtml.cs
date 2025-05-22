using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using Microsoft.EntityFrameworkCore;

public class AdminIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AdminIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public int InstructorCount { get; set; }
    public int PendingReservations { get; set; }
    public string ActiveTerm { get; set; } = "-";
    public int ClassroomCount { get; set; }

    public async Task OnGetAsync()
    {
        InstructorCount = await _context.Users.CountAsync(u => u.Role == "Instructor");
        PendingReservations = await _context.Reservations.CountAsync(r => r.Status == "Pending");

        var today = DateTime.Today;
        var activeTerm = await _context.Terms
            .Where(t => t.StartDate <= today && t.EndDate >= today)
            .OrderBy(t => t.StartDate)
            .FirstOrDefaultAsync();

        ActiveTerm = activeTerm != null ? activeTerm.Name : "No Active Term";
        ClassroomCount = await _context.Classrooms.CountAsync();
    }

}