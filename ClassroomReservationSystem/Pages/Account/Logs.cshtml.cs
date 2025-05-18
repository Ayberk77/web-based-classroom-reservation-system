using ClassroomReservationSystem.Data;
using ClassroomReservationSystem.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

public class AdminLogsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public AdminLogsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public List<SystemLog> SystemLogs { get; set; } = new();
    public List<ErrorLog> ErrorLogs { get; set; } = new();
    public Dictionary<int, string> UserNames { get; set; } = new();

    public async Task OnGetAsync()
    {
        var userIds = await _context.SystemLogs
            .Where(log => log.UserId != null)
            .Select(log => log.UserId!.Value)
            .Distinct()
            .ToListAsync();

        UserNames = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName);

        SystemLogs = await _context.SystemLogs
            .OrderByDescending(log => log.Timestamp)
            .Take(100)
            .ToListAsync();

        ErrorLogs = await _context.ErrorLogs
            .OrderByDescending(err => err.Time)
            .Take(100)
            .ToListAsync();
    }
}
