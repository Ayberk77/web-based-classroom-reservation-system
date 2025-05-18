using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;

public class HolidayService
{
    private readonly CalendarService _calendarService;
    private readonly string _calendarId;
    private readonly IMemoryCache _cache;

    public HolidayService(IConfiguration config, IMemoryCache cache)
    {
        _cache = cache;
        var apiKey = config["GoogleCalendar:ApiKey"];
        _calendarId = config["GoogleCalendar:CalendarId"] ?? "tr.turkish#holiday@group.v.calendar.google.com";

        _calendarService = new CalendarService(new BaseClientService.Initializer()
        {
            ApiKey = apiKey,
            ApplicationName = "ReservationHolidayChecker"
        });
    }

    public async Task<List<DateTime>> GetHolidaysAsync(DateTime start, DateTime end)
    {
        string cacheKey = $"holidays_{start:yyyyMMdd}_{end:yyyyMMdd}";
        if (_cache.TryGetValue(cacheKey, out List<DateTime>? cached) && cached != null)
        {
            return cached;
        }

        var request = _calendarService.Events.List(_calendarId);
        request.TimeMinDateTimeOffset = new DateTimeOffset(start.Date);
        request.TimeMaxDateTimeOffset = new DateTimeOffset(end.Date.AddDays(1));
        request.SingleEvents = true;
        request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

        var response = await request.ExecuteAsync();

        var holidays = response.Items
            .Where(e => e.Start?.Date != null)
            .Select(e => DateTime.Parse(e.Start.Date))
            .ToList();

        _cache.Set(cacheKey, holidays, TimeSpan.FromDays(7));
        return holidays;
    }

    public async Task<bool> IsHolidayAsync(DateTime date)
    {
        var holidays = await GetHolidaysAsync(date, date);
        return holidays.Contains(date.Date);
    }

    public async Task<HashSet<DateTime>> GetHolidaySetAsync(DateTime start, DateTime end)
    {
        var list = await GetHolidaysAsync(start, end);
        return list.ToHashSet();
    }
}