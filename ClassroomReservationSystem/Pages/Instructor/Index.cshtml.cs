using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;

public class InstructorIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public InstructorIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public string InstructorName { get; set; } = string.Empty;
    public string ActiveTerm { get; set; } = string.Empty;
    public int ApprovedCount { get; set; }
    public int PendingCount { get; set; }
    public async Task OnGetAsync()
    {
        var userEmail = User.Identity?.Name;
        var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (instructor == null)
        {
            Console.WriteLine("[DEBUG] Instructor not found.");
            return;
        }

        InstructorName = instructor.FullName;

        var now = DateTime.Now;
        Console.WriteLine($"[DEBUG] Now = {now}");

        var terms = await _context.Terms.ToListAsync();
        foreach (var t in terms)
        {
            Console.WriteLine($"[DEBUG] Term: {t.Name}, Start = {t.StartDate}, End = {t.EndDate}");
            if (t.StartDate <= now && t.EndDate >= now)
                Console.WriteLine($"[DEBUG] --> MATCH: {t.Name}");
        }

        var term = terms.FirstOrDefault(t => t.StartDate <= now && t.EndDate >= now);
        if (term == null)
        {
            Console.WriteLine("[DEBUG] No active term found.");
        }
        else
        {
            Console.WriteLine("[DEBUG] Active term selected: " + term.Name);
        }

        ActiveTerm = term?.Name ?? string.Empty;

        var reservations = await _context.Reservations
            .Where(r => r.UserId == instructor.Id)
            .ToListAsync();

        ApprovedCount = reservations.Count(r => r.Status == "Approved");
        PendingCount = reservations.Count(r => r.Status == "Pending");
    }

}