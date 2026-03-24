namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class RequestQueryEnricherTests
{
    private const string LogPropertyName = "RequestQuery";
    private IHttpContextAccessor _contextAccessor;

    [SetUp]
    public void SetUp()
    {
        var httpContext = new DefaultHttpContext();
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(httpContext);
        _contextAccessor = mock.Object;
    }

    [Test]
    public void EnrichLogWithQuery_WhenHttpRequestContainQuery_ShouldCreateBodyProperty()
    {
        // Arrange
        var query = "?param1=2,list1=test,list1=anothertest";
        _contextAccessor.HttpContext.Request.QueryString = new QueryString(query);
        var queryEnricher = new RequestQueryEnricher(_contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(queryEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has a request query.");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(LogPropertyName);
        evt.Properties[LogPropertyName].LiteralValue().ToString().Should().Be(query);
    }

    [Test]
    public void Enrich_WhenHttpContextIsNull_DoesNotAddProperty()
    {
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns((Microsoft.AspNetCore.Http.HttpContext?)null);
        var contextAccessor = mock.Object;
        var enricher = new RequestQueryEnricher(contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().NotContainKey(LogPropertyName);
    }

    [Test]
    public void Enrich_WhenLogMoreThanOnce_ShouldReadFromHttpContextItems()
    {
        var query = "?a=1&b=2";
        _contextAccessor.HttpContext.Request.QueryString = new QueryString(query);
        var queryEnricher = new RequestQueryEnricher(_contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(queryEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("first");
        log.Information("second");

        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(LogPropertyName);
        evt.Properties[LogPropertyName].LiteralValue().ToString().Should().Be(query);
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
