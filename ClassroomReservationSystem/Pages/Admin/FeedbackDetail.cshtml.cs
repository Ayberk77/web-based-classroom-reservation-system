using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using Microsoft.EntityFrameworkCore;

public class FeedbackDetailModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public FeedbackDetailModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public string ClassroomName { get; set; } = string.Empty;
    public List<FeedbackItem> Feedbacks { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var classroom = await _context.Classrooms.FindAsync(id);
        if (classroom == null) return NotFound();

        ClassroomName = classroom.Name;

        Feedbacks = await _context.Feedbacks
            .Where(f => f.ClassroomId == id)
            .Include(f => f.User)
            .Select(f => new FeedbackItem
            {
                InstructorName = f.User.FullName,
                Rating = f.Rating,
                Comment = f.Comment ?? ""
            }).ToListAsync();

        return Page();
    }

    public class FeedbackItem
    {
        public string InstructorName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
