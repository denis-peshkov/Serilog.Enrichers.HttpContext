namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class RequestQueryEnricherTests
{
    private const string LogPropertyName = "RequestQuery";
    private readonly IHttpContextAccessor _contextAccessor;

    public RequestQueryEnricherTests()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
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
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(query, evt.Properties[LogPropertyName].LiteralValue().ToString());
    }

    [Fact]
    public void Enrich_WhenHttpContextIsNull_DoesNotAddProperty()
    {
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns((Microsoft.AspNetCore.Http.HttpContext?)null);
        var enricher = new RequestQueryEnricher(contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey(LogPropertyName));
    }

    [Fact]
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

        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        Assert.Equal(query, evt.Properties[LogPropertyName].LiteralValue().ToString());
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
