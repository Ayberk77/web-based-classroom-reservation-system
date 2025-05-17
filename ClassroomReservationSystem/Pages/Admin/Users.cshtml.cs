using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.EntityFrameworkCore;

public class AdminUsersModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public AdminUsersModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<User> Instructors { get; set; } = new();

    [BindProperty]
    public InputModel NewInstructor { get; set; } = new();

    public class InputModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public async Task OnGetAsync()
    {
        Instructors = await _context.Users.Where(u => u.Role == "Instructor").ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            Instructors = await _context.Users.Where(u => u.Role == "Instructor").ToListAsync();
            return Page();
        }

        var user = new User
        {
            FullName = NewInstructor.FullName,
            Email = NewInstructor.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewInstructor.Password),
            Role = "Instructor"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var instructor = await _context.Users.FindAsync(id);
        if (instructor != null && instructor.Role == "Instructor")
        {
            _context.Users.Remove(instructor);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}