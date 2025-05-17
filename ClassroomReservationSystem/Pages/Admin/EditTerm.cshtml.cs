using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

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

        existing.Name = Term.Name;
        existing.StartDate = Term.StartDate;
        existing.EndDate = Term.EndDate;

        await _context.SaveChangesAsync();
        return RedirectToPage("Terms");
    }
}