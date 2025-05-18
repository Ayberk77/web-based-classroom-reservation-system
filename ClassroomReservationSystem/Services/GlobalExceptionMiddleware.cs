public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public GlobalExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, ILogService logService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var path = context.Request.Path;
            var method = context.Request.Method;
            var contextInfo = $"{method} {path}";
            await logService.LogErrorAsync(ex, contextInfo);


            context.Response.Redirect("/Error");
        }
        
        if (context.Response.StatusCode == 403)
        {
            var logContext = $"{context.Request.Method} {context.Request.Path} - UNAUTHORIZED ACCESS";
            await logService.LogErrorAsync(new Exception("Unauthorized access attempt"), logContext);
        }

    }
}
