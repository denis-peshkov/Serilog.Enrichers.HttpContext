namespace Serilog.Enrichers;

/// <inheritdoc/>
public class RequestHeaderEnricher : ILogEventEnricher
{
    private readonly string _itemKey;
    private readonly string _propertyName;
    private readonly string _headerKey;
    private readonly IHttpContextAccessor _contextAccessor;

    public RequestHeaderEnricher(string headerKey, string propertyName)
        : this(headerKey, propertyName, new HttpContextAccessor())
    {
    }

    internal RequestHeaderEnricher(string headerKey, string propertyName, IHttpContextAccessor contextAccessor)
    {
        _headerKey = headerKey;
        _itemKey = $"Serilog_{headerKey}";
        _propertyName = string.IsNullOrWhiteSpace(propertyName)
            ? headerKey.Replace("-", "")
            : propertyName;
        _contextAccessor = contextAccessor;
    }

    internal RequestHeaderEnricher(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    /// <inheritdoc/>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }
        if (httpContext.Items[_itemKey] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        var headerValue = httpContext.Request.Headers[_headerKey].ToString();
        headerValue = string.IsNullOrWhiteSpace(headerValue) ? null : headerValue;

        var logProperty = propertyFactory.CreateProperty(_propertyName, headerValue);
        httpContext.Items[_itemKey] = logProperty;

        logEvent.AddOrUpdateProperty(logProperty);
    }
}
