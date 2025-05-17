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
    public int? SelectedClassroomId { get; set; }

    public class ReservationViewModel
    {
        public string InstructorName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public async Task OnGetAsync(int? classroomId)
    {
        Classrooms = await _context.Classrooms.OrderBy(c => c.Name).ToListAsync();
        SelectedClassroomId = classroomId;

        var query = _context.Reservations
            .Include(r => r.Classroom)
            .Include(r => r.User)
            .Where(r => r.Status == "Approved");

        if (classroomId.HasValue)
            query = query.Where(r => r.ClassroomId == classroomId);

        Reservations = await query
            .Select(r => new ReservationViewModel
            {
                InstructorName = r.User != null ? r.User.FullName : "",
                StartTime = r.StartTime,
                EndTime = r.EndTime
            })
            .ToListAsync();
    }
}
