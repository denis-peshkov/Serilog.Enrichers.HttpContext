namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class RequestBodyEnricherTests
{
    private const string LogPropertyName = "RequestBody";
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
    public void EnrichLogWithBody_WhenHttpRequestContainBody_ShouldCreateBodyProperty()
    {
        // Arrange
        var body = "{ \"a1\": 10, \"a2\": \"vvv\" }";
        using var memoryStream = new MemoryStream();
        UpdateMemoryStream(memoryStream, body);
        memoryStream.Seek(0, SeekOrigin.Begin);
        _contextAccessor.HttpContext.Request.Body = memoryStream;
        var bodyEnricher = new RequestBodyEnricher(_contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(bodyEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has a request body.");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(LogPropertyName);
        // Assert.Equal(body, evt.Properties[LogPropertyName].LiteralValue().ToString()); // todo: fix some strange bug
    }




    [Test]
    public void Enrich_WhenHttpContextIsNull_DoesNotAddProperty()
    {
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns((Microsoft.AspNetCore.Http.HttpContext?)null);
        var contextAccessor = mock.Object;
        var enricher = new RequestBodyEnricher(contextAccessor);

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
    public void WithRequestBody_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithRequestBody()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var act = () => logger.Information("LOG");
        act.Should().NotThrow();
    }

    private static void UpdateMemoryStream(MemoryStream memoryStream, string responseBody)
    {
        memoryStream.SetLength(0);
        var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        writer.Write(responseBody);
        writer.Flush();
    }
}
