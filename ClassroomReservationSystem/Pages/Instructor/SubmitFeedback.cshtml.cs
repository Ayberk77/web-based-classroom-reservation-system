using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

public class SubmitFeedbackModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _config;

    public SubmitFeedbackModel(ApplicationDbContext context, IEmailSender emailSender, IConfiguration config)
    {
        _context = context;
        _emailSender = emailSender;
        _config = config;
    }


    public List<SelectListItem> Classrooms { get; set; } = new();

    [BindProperty]
    public FeedbackInputModel Input { get; set; } = new();



    public class FeedbackInputModel
    {
        public int ClassroomId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        var userEmail = User.Identity?.Name;
        var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (instructor == null) return;

        Classrooms = await _context.Reservations
            .Where(r => r.UserId == instructor.Id && r.Classroom != null)
            .Select(r => r.Classroom!)
            .Distinct()
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userEmail = User.Identity?.Name;
        var instructor = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (instructor == null) return Unauthorized();

        var feedback = new Feedback
        {
            ClassroomId = Input.ClassroomId,
            InstructorId = instructor.Id,
            Rating = Input.Rating,
            Comment = Input.Comment
        };

        _context.Feedbacks.Add(feedback);
        await _context.SaveChangesAsync();

        var adminEmail = _config["SmtpSettings:AdminEmail"] ?? throw new InvalidOperationException("Admin email not configured");

        var classroom = await _context.Classrooms.FindAsync(Input.ClassroomId);

        await _emailSender.SendEmailAsync(
            adminEmail,
            "New Feedback Submitted",
            $"Instructor: {instructor.FullName}\n" +
            $"Classroom: {classroom?.Name ?? "Unknown"}\n" +
            $"Rating: {Input.Rating}\n" +
            $"Comment: {Input.Comment}"
        );




        TempData["Message"] = "Feedback submitted successfully.";
        return RedirectToPage(); // redirect to clear form
    }
}
