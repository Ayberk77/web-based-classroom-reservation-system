using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

public class AdminTermsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public AdminTermsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<Term> Terms { get; set; } = new();

    [BindProperty]
    public Term NewTerm { get; set; } = new Term
    {
        StartDate = DateTime.Today,
        EndDate = DateTime.Today.AddDays(1)
    };


    public async Task OnGetAsync()
    {
        Terms = await _context.Terms.OrderByDescending(t => t.StartDate).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Terms = await _context.Terms.OrderByDescending(t => t.StartDate).ToListAsync();
            return Page();
        }

         bool conflict = await _context.Terms
            .AnyAsync(t =>
                NewTerm.StartDate <= t.EndDate &&
                NewTerm.EndDate >= t.StartDate);

        if (conflict)
        {
            ModelState.AddModelError(string.Empty, "This term overlaps with an existing term.");
            Terms = await _context.Terms.OrderByDescending(t => t.StartDate).ToListAsync();
            return Page();
        }

        _context.Terms.Add(NewTerm);
        await _context.SaveChangesAsync();
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var term = await _context.Terms.FindAsync(id);
        if (term != null)
        {
            _context.Terms.Remove(term);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}