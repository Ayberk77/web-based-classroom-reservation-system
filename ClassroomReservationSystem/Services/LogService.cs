using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

public interface ILogService
{
    Task LogActionAsync(int? userId, string action, string status);
    Task LogErrorAsync(Exception ex, string? contextInfo = null);
}

public class LogService : ILogService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogActionAsync(int? userId, string action, string status)
    {
        var log = new SystemLog
        {
            UserId = userId,
            Action = action,
            Status = status,
            Timestamp = DateTime.UtcNow
        };

        _context.SystemLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogErrorAsync(Exception ex, string? contextInfo = null)
    {
        var http = _httpContextAccessor.HttpContext;

        var error = new ErrorLog
        {
            ExceptionMessage = ex.Message,
            StackTrace = ex.StackTrace ?? "",
            Time = DateTime.UtcNow,
            UserContext = contextInfo ?? "General",
            RequestPath = http?.Request?.Path,
            QueryString = http?.Request?.QueryString.Value,
            UserAgent = http?.Request?.Headers["User-Agent"].ToString(),
            IPAddress = http?.Connection?.RemoteIpAddress?.ToString(),
            UserEmail = http?.User?.FindFirst(ClaimTypes.Email)?.Value ?? "Anonymous"
        };

        _context.ErrorLogs.Add(error);
        await _context.SaveChangesAsync();
    }
}
