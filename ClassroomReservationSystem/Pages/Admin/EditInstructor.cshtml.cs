using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

public class EditInstructorModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditInstructorModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; } // optional
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null || user.Role != "Instructor") return NotFound();

        Input = new InputModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _context.Users.FindAsync(Input.Id);
        if (user == null || user.Role != "Instructor") return NotFound();

        user.FullName = Input.FullName;
        user.Email = Input.Email;

        if (!string.IsNullOrWhiteSpace(Input.Password))
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Input.Password);
        }

        await _context.SaveChangesAsync();
        return RedirectToPage("Users");
    }
}
