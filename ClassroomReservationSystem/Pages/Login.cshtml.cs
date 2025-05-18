using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogService _logService;

    public LoginModel(ApplicationDbContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public class InputModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))
                return RedirectToPage("/Admin/Index");
            else if (User.IsInRole("Instructor"))
                return RedirectToPage("/Instructor/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var user = _context.Users.FirstOrDefault(u => u.Email == Input.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash))
        {
            await _logService.LogActionAsync(null, $"Failed login attempt with email {Input.Email}", "Failed");
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("FullName", user.FullName)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties { IsPersistent = true };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), authProperties);

        await _logService.LogActionAsync(user.Id, "Login", "Success");

        return RedirectToPage(user.Role == "Admin" ? "/Admin/Index" : "/Instructor/Index");
    }
}