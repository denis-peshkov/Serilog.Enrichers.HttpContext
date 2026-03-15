namespace Serilog.Enrichers.HttpContext.Tests.Extensions;

public class HttpContextLoggerConfigurationExtensionsTests
{
    [Fact]
    public void WithClientIp_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithClientIp());
        Assert.Equal("enrichmentConfiguration", ex.ParamName);
    }

    [Fact]
    public void WithMemoryUsage_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithMemoryUsage());
        Assert.Equal("enrichmentConfiguration", ex.ParamName);
    }

    [Fact]
    public void WithMemoryUsageExact_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithMemoryUsageExact());
        Assert.Equal("enrichmentConfiguration", ex.ParamName);
    }

    [Fact]
    public void WithRequestBody_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithRequestBody());
        Assert.Equal("enrichmentConfiguration", ex.ParamName);
    }

    [Fact]
    public void WithRequestQuery_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithRequestQuery());
        Assert.Equal("enrichmentConfiguration", ex.ParamName);
    }

    [Fact]
    public void WithRequestHeader_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithRequestHeader("X-Header"));
        Assert.Equal("enrichmentConfiguration", ex.ParamName);
    }

    [Fact]
    public void WithRequestHeader_WhenHeaderNameIsNull_ThrowsArgumentNullException()
    {
        var config = new LoggerConfiguration();
        var ex = Assert.Throws<ArgumentNullException>(() =>
            config.Enrich.WithRequestHeader(null!));
        Assert.Equal("headerName", ex.ParamName);
    }

    [Fact]
    public void WithMemoryUsage_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithMemoryUsage()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var ex = Record.Exception(() => logger.Information("LOG"));
        Assert.Null(ex);
    }

    [Fact]
    public void WithMemoryUsageExact_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithMemoryUsageExact()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var ex = Record.Exception(() => logger.Information("LOG"));
        Assert.Null(ex);
    }

    [Fact]
    public void WithRequestBody_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithRequestBody()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var ex = Record.Exception(() => logger.Information("LOG"));
        Assert.Null(ex);
    }

    [Fact]
    public void WithRequestQuery_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithRequestQuery()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var ex = Record.Exception(() => logger.Information("LOG"));
        Assert.Null(ex);
    }
}
