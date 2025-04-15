namespace Serilog.Enrichers;

/// <inheritdoc/>
public class MemoryUsageEnricher : ILogEventEnricher
{
    private const string PROPERTY_NAME = "MemoryUsage";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var currentMemoryInBytes = Process.GetCurrentProcess().WorkingSet64;

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(PROPERTY_NAME, currentMemoryInBytes));
    }
}
