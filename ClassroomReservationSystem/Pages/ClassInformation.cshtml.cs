using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

public class ClassInformationModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public ClassInformationModel(ApplicationDbContext context) => _context = context;

    public List<Classroom> Classrooms { get; set; } = new();
    public List<ReservationViewModel> Reservations { get; set; } = new();
    public List<FeedbackViewModel> Feedbacks { get; set; } = new();

    public int? SelectedClassroomId { get; set; }
    public double? AverageRating { get; set; }

    public class ReservationViewModel
    {
        public string InstructorName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }

    public class FeedbackViewModel
    {
        public string InstructorName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public async Task OnGetAsync(int? classroomId)
    {
        Classrooms = await _context.Classrooms.OrderBy(c => c.Name).ToListAsync();

        if (!Classrooms.Any())
        {
            SelectedClassroomId = 0;
            Reservations = new();
            return;
        }

        SelectedClassroomId = classroomId ?? Classrooms.First().Id;

        var query = _context.Reservations
            .Include(r => r.Classroom)
            .Include(r => r.User)
            .Where(r => r.Status == "Approved" && r.ClassroomId == SelectedClassroomId);

        Reservations = await query
            .Select(r => new ReservationViewModel
            {
                InstructorName = r.User != null ? r.User.FullName : "",
                StartTime = r.StartTime,
                EndTime = r.EndTime,
            })
            .ToListAsync();

        if (User.IsInRole("Admin"))
        {
            var feedbacks = await _context.Feedbacks
                .Include(f => f.Instructor)
                .Where(f => f.ClassroomId == SelectedClassroomId)
                .ToListAsync();

            Feedbacks = feedbacks.Select(f => new FeedbackViewModel
            {
                InstructorName = f.Instructor?.FullName ?? "Unknown",
                Rating = f.Rating,
                Comment = f.Comment ?? "",
                CreatedAt = f.CreatedAt
            }).ToList();

            AverageRating = Feedbacks.Any() ? Feedbacks.Average(f => f.Rating) : null;
        }
    }
}
