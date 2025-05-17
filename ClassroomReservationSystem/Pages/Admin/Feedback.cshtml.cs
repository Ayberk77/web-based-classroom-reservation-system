using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;

public class AdminFeedbackModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public AdminFeedbackModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<FeedbackSummaryViewModel> FeedbackSummary { get; set; } = new();

    public async Task OnGetAsync()
    {
        FeedbackSummary = await _context.Feedbacks
            .GroupBy(f => f.ClassroomId)
            .Select(g => new FeedbackSummaryViewModel
            {
                ClassroomId = g.Key,
                ClassroomName = g.First().Classroom.Name,
                AverageRating = g.Average(x => x.Rating),
                Count = g.Count()
            }).ToListAsync();
    }

    public class FeedbackSummaryViewModel
    {
        public int ClassroomId { get; set; }
        public string ClassroomName { get; set; } = string.Empty;
        public double AverageRating { get; set; }
        public int Count { get; set; }
    }
}
