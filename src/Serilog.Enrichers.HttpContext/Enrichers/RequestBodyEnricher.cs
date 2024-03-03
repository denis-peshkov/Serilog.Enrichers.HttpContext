namespace Serilog.Enrichers;

/// <inheritdoc/>
public class RequestBodyEnricher : ILogEventEnricher
{
    private const string REQUEST_BODY_ITEM_KEY = "Serilog_RequestBody";
    private const string REQUEST_BODY_PROPERTY_NAME = "RequestBody";
    private readonly IHttpContextAccessor _contextAccessor;

    public RequestBodyEnricher()
        : this(new HttpContextAccessor())
    {
    }

    internal RequestBodyEnricher(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        if (httpContext.Items[REQUEST_BODY_ITEM_KEY] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        var requestBody = GetStringAsync(httpContext.Request.Body).GetAwaiter().GetResult();

        var requestBodyProperty = new LogEventProperty(REQUEST_BODY_PROPERTY_NAME, new ScalarValue(requestBody));
        logEvent.AddOrUpdateProperty(requestBodyProperty);

        httpContext.Items.Add(REQUEST_BODY_ITEM_KEY, requestBodyProperty);
    }

    private static async Task<string> GetStringAsync(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
