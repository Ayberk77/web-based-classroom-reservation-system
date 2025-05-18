using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

public class EditTermModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditTermModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Term Term { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var term = await _context.Terms.FindAsync(id);
        if (term == null)
            return NotFound();

        Term = term;
        return Page();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var existing = await _context.Terms.FindAsync(Term.Id);
        if (existing == null)
            return NotFound();

        bool conflict = await _context.Terms
            .Where(t => t.Id != Term.Id) 
            .AnyAsync(t =>
                Term.StartDate <= t.EndDate &&
                Term.EndDate >= t.StartDate);

        if (conflict)
        {
            ModelState.AddModelError(string.Empty, "This term overlaps with another existing term.");
            return Page();
        }

        existing.Name = Term.Name;
        existing.StartDate = Term.StartDate;
        existing.EndDate = Term.EndDate;

        await _context.SaveChangesAsync();
        return RedirectToPage("Terms");
    }
}