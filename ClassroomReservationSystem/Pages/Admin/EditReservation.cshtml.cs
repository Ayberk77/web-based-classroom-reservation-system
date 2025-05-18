using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

public class EditReservationModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditReservationModel(ApplicationDbContext context) => _context = context;

    [BindProperty]
    public Reservation Reservation { get; set; } = new();

    [BindProperty]
    public bool ApproveAfterEdit { get; set; } = false;

    public List<SelectListItem> ClassroomOptions { get; set; } = new();

    private readonly List<DateTime> holidays = new()
    {
        new DateTime(2025, 1, 1),
        new DateTime(2025, 4, 23),
        new DateTime(2025, 5, 1),
        new DateTime(2025, 5, 19)
    };

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Reservation = await _context.Reservations.FindAsync(id) ?? throw new InvalidOperationException("Reservation not found");

        ClassroomOptions = await _context.Classrooms
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        if (Reservation.StartTime >= Reservation.EndTime)
        {
            ModelState.AddModelError(string.Empty, "Start time must be before end time.");
            ClassroomOptions = await _context.Classrooms
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            return Page();
        }

        var dbReservation = await _context.Reservations.FindAsync(Reservation.Id);
        if (dbReservation == null) return NotFound();

        bool isHoliday = holidays.Contains(Reservation.StartTime.Date);

        bool isConflict = await _context.Reservations
            .Where(o => o.Id != Reservation.Id && o.ClassroomId == Reservation.ClassroomId && o.Status == "Approved")
            .AnyAsync(o => Reservation.StartTime < o.EndTime && o.StartTime < Reservation.EndTime);

        if (ApproveAfterEdit && (isHoliday || isConflict))
        {
            ModelState.AddModelError(string.Empty, "Cannot approve: reservation is conflicting or on a holiday.");
            ClassroomOptions = await _context.Classrooms
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
            return Page();
        }

        dbReservation.ClassroomId = Reservation.ClassroomId;
        dbReservation.StartTime = Reservation.StartTime;
        dbReservation.EndTime = Reservation.EndTime;
        dbReservation.Note = Reservation.Note;
        dbReservation.Status = ApproveAfterEdit ? "Approved" : "Pending";

        await _context.SaveChangesAsync();

        return RedirectToPage("/Admin/Reservations");
    }
}