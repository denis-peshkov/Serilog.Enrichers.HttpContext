namespace Serilog.Extensions;

/// <summary>
///   Extension methods for setting up client IP, client agent and correlation identifier enrichers <see cref="LoggerEnrichmentConfiguration"/>.
/// </summary>
public static class HttpContextLoggerConfigurationExtensions
{
    /// <summary>
    ///   Registers the client IP enricher to enrich logs with client IP with 'X-forwarded-for'
    ///   header information.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="headerName">
    ///   Set the 'X-Forwarded-For' header in case if service is behind proxy server. Default value
    ///   is 'x-forwarded-for'.
    /// </param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithClientIp(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        string headerName = "x-forwarded-for")
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        return enrichmentConfiguration.With(new ClientIpEnricher(headerName));
    }

    /// <summary>
    ///   Registers the memory enricher to enrich logs with used memory in process.
    /// </summary>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithMemoryUsage(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        return enrichmentConfiguration.With(new MemoryUsageEnricher());
    }

    /// <summary>
    ///   Registers the memory enricher to enrich logs with used memory in process exact value.
    /// </summary>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithMemoryUsageExact(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        return enrichmentConfiguration.With(new MemoryUsageExactEnricher());
    }

    /// <summary>
    ///   Registers the HTTP request header enricher to enrich logs with the header value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithRequestBody(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        return enrichmentConfiguration.With(new RequestBodyEnricher());
    }

    /// <summary>
    ///   Registers the HTTP request header enricher to enrich logs with the header value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <param name="propertyName">The property name of log</param>
    /// <param name="headerName">The header name to log its value</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <exception cref="ArgumentNullException">headerName</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithRequestHeader(
        this LoggerEnrichmentConfiguration enrichmentConfiguration,
        string headerName,
        string propertyName = null)
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        if (headerName == null)
        {
            throw new ArgumentNullException(nameof(headerName));
        }

        return enrichmentConfiguration.With(new RequestHeaderEnricher(headerName, propertyName));
    }

    /// <summary>
    ///   Registers the HTTP request header enricher to enrich logs with the header value.
    /// </summary>
    /// <param name="enrichmentConfiguration">The enrichment configuration.</param>
    /// <exception cref="ArgumentNullException">enrichmentConfiguration</exception>
    /// <returns>The logger configuration so that multiple calls can be chained.</returns>
    public static LoggerConfiguration WithRequestQuery(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        if (enrichmentConfiguration == null)
        {
            throw new ArgumentNullException(nameof(enrichmentConfiguration));
        }

        return enrichmentConfiguration.With(new RequestQueryEnricher());
    }
}
