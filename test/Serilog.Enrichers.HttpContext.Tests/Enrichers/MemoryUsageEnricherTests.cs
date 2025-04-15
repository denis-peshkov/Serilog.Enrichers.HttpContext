namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class MemoryUsageEnricherTests
{
    private const string LogPropertyName = "MemoryUsage";
    private readonly IHttpContextAccessor _contextAccessor;

    public MemoryUsageEnricherTests()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
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
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.True(int.Parse(evt.Properties[LogPropertyName].LiteralValue().ToString()) > 10_999_999);
    }
}
