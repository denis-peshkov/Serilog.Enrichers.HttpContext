namespace Serilog.Enrichers;

/// <inheritdoc/>
public class RequestBodyEnricher : ILogEventEnricher
{
    private const string ITEM_KEY = $"Serilog_{PROPERTY_NAME}";
    /// <summary>The property name added to enriched log events.</summary>
    private const string PROPERTY_NAME = "RequestBody";
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

        if (httpContext.Items[ITEM_KEY] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        httpContext.Request.EnableBuffering();
        using var memoryStream = new MemoryStream();
        // httpContext.Request.Body.CopyTo(memoryStream); // throw InvalidOperationException with Message: Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true instead.
        httpContext.Request.Body.CopyToAsync(memoryStream).GetAwaiter().GetResult();
        httpContext.Request.Body.Position = 0;
        memoryStream.Position = 0;
        var requestBody = Encoding.UTF8.GetString(memoryStream.ToArray());

        var requestBodyProperty = propertyFactory.CreateProperty(PROPERTY_NAME, requestBody);
        logEvent.AddOrUpdateProperty(requestBodyProperty);

        httpContext.Items.Add(ITEM_KEY, requestBodyProperty);
    }
}
