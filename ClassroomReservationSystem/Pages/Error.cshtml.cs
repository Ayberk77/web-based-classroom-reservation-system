using Microsoft.AspNetCore.Mvc.RazorPages;

public class ErrorModel : PageModel
{
    private readonly ILogService _logService;

    public string ErrorMessage { get; set; } = "An unknown error occurred.";

    public ErrorModel(ILogService logService)
    {
        _logService = logService;
    }

    public async Task OnGet(int? code = null)
    {
        var user = User.Identity?.Name ?? "Anonymous";
        var path = Request.Query["aspxerrorpath"].ToString() ?? HttpContext.Request.Path;

        if (code.HasValue)
        {
            switch (code.Value)
            {
                case 403:
                    ErrorMessage = "403 - You do not have permission.";
                    await _logService.LogErrorAsync(
                        new Exception("403 - Access Denied"),
                        $"AccessDenied → {user} tried to access {path}"
                    );
                    break;

                case 404:
                    ErrorMessage = "404 - Page not found.";
                    await _logService.LogErrorAsync(
                        new Exception("404 - Not Found"),
                        $"NotFound → {user} tried to access {path}"
                    );
                    break;

                default:
                    ErrorMessage = $"{code.Value} - Unexpected error.";
                    await _logService.LogErrorAsync(
                        new Exception($"HTTP {code.Value} error"),
                        $"Unexpected → {user} at {path}"
                    );
                    break;
            }
        }
        else
        {
            ErrorMessage = "500 - Internal server error.";
            await _logService.LogErrorAsync(
                new Exception("500 - Server Error"),
                $"Unhandled exception by {user} at {path}"
            );
        }
    }
}
