namespace Serilog.Extensions;

/// <summary>
/// Extends <see cref="IApplicationBuilder"/> with methods for configuring Serilog features.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds middleware for streamlined request logging. Instead of writing HTTP request information
    /// like method, path, timing, status code and exception details
    /// in several events, this middleware collects information during the request (including from
    /// <see cref="IDiagnosticContext"/>), and writes a single event at request completion. Add this
    /// in <c>Startup.cs</c> before any handlers whose activities should be logged.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder.</returns>
    public static IApplicationBuilder UseSerilogMemoryUsageExact(
        this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        return app.UseMiddleware<RequestMemoryLoggingMiddleware>();
    }
}
