using System.Diagnostics;

namespace MinimalLibrary.Api.Middleware;

public class TimerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TimerMiddleware> _logger;

    public TimerMiddleware(RequestDelegate next, ILogger<TimerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopWatch = new Stopwatch();
        
        try
        {
            stopWatch.Start();
            await _next(context);
        }
        finally
        {
            stopWatch.Stop();
            _logger.LogInformation($"{context.GetEndpoint()!.DisplayName} request completed in: {stopWatch.ElapsedMilliseconds} ms");
        }
    }
}