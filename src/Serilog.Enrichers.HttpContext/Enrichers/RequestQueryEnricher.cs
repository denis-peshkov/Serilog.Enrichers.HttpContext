namespace Serilog.Enrichers;

/// <inheritdoc/>
public class RequestQueryEnricher : ILogEventEnricher
{
    private const string ITEM_KEY = $"Serilog_{PROPERTY_NAME}";
    private const string PROPERTY_NAME = "RequestQuery";
    private readonly IHttpContextAccessor _contextAccessor;

    public RequestQueryEnricher()
        : this(new HttpContextAccessor())
    {
    }

    internal RequestQueryEnricher(IHttpContextAccessor contextAccessor)
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

        var requestQuery = httpContext.Request.QueryString;

        var requestBodyProperty = new LogEventProperty(PROPERTY_NAME, new ScalarValue(requestQuery));
        logEvent.AddOrUpdateProperty(requestBodyProperty);

        httpContext.Items.Add(ITEM_KEY, requestBodyProperty);
    }
}
