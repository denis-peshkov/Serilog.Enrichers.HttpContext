namespace Serilog.Enrichers;

/// <inheritdoc/>
public class RequestBodyEnricher : ILogEventEnricher
{
    private const string ITEM_KEY = "Serilog_RequestBody";
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
        try
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

            var requestBody = GetStringAsync(httpContext.Request.Body).GetAwaiter().GetResult();

            var requestBodyProperty = new LogEventProperty(PROPERTY_NAME, new ScalarValue(requestBody));
            logEvent.AddOrUpdateProperty(requestBodyProperty);

            httpContext.Items.Add(ITEM_KEY, requestBodyProperty);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }

    }

    private static async Task<string> GetStringAsync(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
