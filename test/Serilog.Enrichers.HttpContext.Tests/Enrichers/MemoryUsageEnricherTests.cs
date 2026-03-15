namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class MemoryUsageEnricherTests
{
    private const string LogPropertyName = "MemoryUsage";
    private IHttpContextAccessor _contextAccessor;

    [SetUp]
    public void SetUp()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Test]
    public void EnrichLogWithMemory_WhenHttpRequestExists_ShouldCreateMemoryProperty()
    {
        // Arrange
        var memoryEnricher = new MemoryUsageEnricher();

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(memoryEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has a memory query.");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(LogPropertyName);
        int.Parse(evt.Properties[LogPropertyName].LiteralValue().ToString()!).Should().BeGreaterThan(10_999_999);
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
}
