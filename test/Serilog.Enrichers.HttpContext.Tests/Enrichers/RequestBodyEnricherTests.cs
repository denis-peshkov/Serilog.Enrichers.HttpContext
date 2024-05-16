namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class RequestBodyEnricherTests
{
    private const string LogPropertyName = "RequestBody";
    private readonly IHttpContextAccessor _contextAccessor;

    public RequestBodyEnricherTests()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Fact]
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
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey(LogPropertyName));
        // Assert.Equal(body, evt.Properties[LogPropertyName].LiteralValue().ToString()); // todo: fix some strange bug
    }




    private static void UpdateMemoryStream(MemoryStream memoryStream, string responseBody)
    {
        memoryStream.SetLength(0);
        var writer = new StreamWriter(memoryStream, Encoding.UTF8);
        writer.Write(responseBody);
        writer.Flush();
    }
}
