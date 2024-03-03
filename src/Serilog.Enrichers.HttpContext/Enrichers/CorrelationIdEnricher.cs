namespace Serilog.Enrichers;

/// <inheritdoc/>
public class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CORRELATION_ID_ITEM_KEY = "Serilog_CorrelationId";
    private const string CORRELATION_ID_PROPERTY_NAME = "CorrelationId";
    private readonly string _headerKey;
    private readonly bool _addValueIfHeaderAbsence;
    private readonly IHttpContextAccessor _contextAccessor;

    public CorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence)
        : this(headerKey, addValueIfHeaderAbsence, new HttpContextAccessor())
    {
    }

    internal CorrelationIdEnricher(string headerKey, bool addValueIfHeaderAbsence, IHttpContextAccessor contextAccessor)
    {
        _headerKey = headerKey;
        _addValueIfHeaderAbsence = addValueIfHeaderAbsence;
        _contextAccessor = contextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }

        if (httpContext.Items[CORRELATION_ID_ITEM_KEY] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        var header = httpContext.Request.Headers[_headerKey].ToString();
        var correlationId = !string.IsNullOrWhiteSpace(header)
            ? header
            : _addValueIfHeaderAbsence ? Guid.NewGuid().ToString() : null;

        var correlationIdProperty = new LogEventProperty(CORRELATION_ID_PROPERTY_NAME, new ScalarValue(correlationId));
        logEvent.AddOrUpdateProperty(correlationIdProperty);

        httpContext.Items.Add(CORRELATION_ID_ITEM_KEY, correlationIdProperty);
    }
}
