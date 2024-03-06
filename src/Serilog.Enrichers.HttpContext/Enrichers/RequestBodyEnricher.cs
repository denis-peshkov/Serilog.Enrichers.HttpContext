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

            httpContext.Request.EnableBuffering();
            using var memoryStream = new MemoryStream();
            httpContext.Request.Body.CopyTo(memoryStream);
            httpContext.Request.Body.Position = 0;
            memoryStream.Position = 0;
            var requestBody = Encoding.UTF8.GetString(memoryStream.ToArray());

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
}
