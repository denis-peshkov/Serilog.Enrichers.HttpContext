namespace Serilog.Enrichers;

/// <inheritdoc/>
public class MemoryUsageExactEnricher : ILogEventEnricher
{
    internal const string ITEM_KEY = $"Serilog_{PROPERTY_NAME}";
    internal const string PROPERTY_NAME = "MemoryUsageExact";
    private readonly IHttpContextAccessor _contextAccessor;

    public MemoryUsageExactEnricher()
        : this(new HttpContextAccessor())
    {
    }

    internal MemoryUsageExactEnricher(IHttpContextAccessor contextAccessor)
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
            var start = long.Parse(logEventProperty.Value.ToString());
            var spentMemory = Process.GetCurrentProcess().WorkingSet64 - start;
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(PROPERTY_NAME, spentMemory <0 ? 0 : spentMemory));
        }
    }
}
