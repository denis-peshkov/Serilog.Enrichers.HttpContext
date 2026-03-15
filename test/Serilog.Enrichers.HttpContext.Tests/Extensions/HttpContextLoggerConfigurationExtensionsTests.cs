namespace Serilog.Enrichers.HttpContext.Tests.Extensions;

public class HttpContextLoggerConfigurationExtensionsTests
{
    [Test]
    public void WithClientIp_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var act = () => ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithClientIp();
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("enrichmentConfiguration");
    }

    [Test]
    public void WithMemoryUsage_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var act = () => ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithMemoryUsage();
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("enrichmentConfiguration");
    }

    [Test]
    public void WithMemoryUsageExact_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var act = () => ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithMemoryUsageExact();
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("enrichmentConfiguration");
    }

    [Test]
    public void WithRequestBody_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var act = () => ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithRequestBody();
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("enrichmentConfiguration");
    }

    [Test]
    public void WithRequestQuery_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var act = () => ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithRequestQuery();
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("enrichmentConfiguration");
    }

    [Test]
    public void WithRequestHeader_WhenEnrichmentConfigurationIsNull_ThrowsArgumentNullException()
    {
        var act = () => ((Serilog.Configuration.LoggerEnrichmentConfiguration)null!).WithRequestHeader("X-Header");
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("enrichmentConfiguration");
    }

    [Test]
    public void WithRequestHeader_WhenHeaderNameIsNull_ThrowsArgumentNullException()
    {
        var config = new LoggerConfiguration();
        var act = () => config.Enrich.WithRequestHeader(null!);
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("headerName");
    }

    [Test]
    public void WithMemoryUsage_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithMemoryUsage()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var act = () => logger.Information("LOG");
        act.Should().NotThrow();
    }

    [Test]
    public void WithMemoryUsageExact_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithMemoryUsageExact()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var act = () => logger.Information("LOG");
        act.Should().NotThrow();
    }

    [Test]
    public void WithRequestBody_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithRequestBody()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var act = () => logger.Information("LOG");
        act.Should().NotThrow();
    }

    [Test]
    public void WithRequestQuery_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithRequestQuery()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var act = () => logger.Information("LOG");
        act.Should().NotThrow();
    }
}
