namespace Serilog.Middlewares;

public class RequestMemoryLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestMemoryLoggingMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    // ReSharper disable once UnusedMember.Global
    public async Task Invoke(HttpContext httpContext)
    {
        if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

        var startMemory = Process.GetCurrentProcess().WorkingSet64;

        var requestQueryProperty = new LogEventProperty(MemoryUsageExactEnricher.PROPERTY_NAME, new ScalarValue(startMemory));
        httpContext.Items.Add(MemoryUsageExactEnricher.ITEM_KEY, requestQueryProperty);

        await _next(httpContext);
    }
}
