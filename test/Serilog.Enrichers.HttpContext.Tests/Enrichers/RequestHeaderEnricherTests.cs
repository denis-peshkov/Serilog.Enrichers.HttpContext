namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class RequestHeaderEnricherTests
{
    private IHttpContextAccessor _contextAccessor;

    [SetUp]
    public void SetUp()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Test]
    public void EnrichLogWithClientHeader_WhenHttpRequestContainHeader_ShouldCreateNamedHeaderValueProperty()
    {
        // Arrange
        var headerKey = "RequestId";
        var propertyName = "HttpRequestId";
        var headerValue = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext.Request.Headers[headerKey] = headerValue;

        var clientHeaderEnricher = new RequestHeaderEnricher(headerKey, propertyName, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"First testing log enricher.");
        log.Information(@"Second testing log enricher.");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(propertyName);
        evt.Properties[propertyName].LiteralValue().ToString().Should().Be(headerValue);
    }

    [Test]
    public void EnrichLogWithClientHeader_WhenHttpRequestContainHeader_ShouldCreateHeaderValueProperty()
    {
        // Arrange
        var headerKey = "RequestId";
        var headerValue = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext.Request.Headers[headerKey] = headerValue;

        var clientHeaderEnricher = new RequestHeaderEnricher(headerKey, propertyName:string.Empty, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"First testing log enricher.");
        log.Information(@"Second testing log enricher.");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(headerKey);
        evt.Properties[headerKey].LiteralValue().ToString().Should().Be(headerValue);
    }

    [Test]
    public void EnrichLogWithMulitpleClientHeaderEnricher_WhenHttpRequestContainHeaders_ShouldCreateHeaderValuesProperty()
    {
        // Arrange
        var headerKey1 = "Header1";
        var headerKey2 = "User-Agent";
        var headerValue1 = Guid.NewGuid().ToString();
        var headerValue2 = Guid.NewGuid().ToString();
        _contextAccessor.HttpContext.Request.Headers[headerKey1] = headerValue1;
        _contextAccessor.HttpContext.Request.Headers[headerKey2] = headerValue2;

        var clientHeaderEnricher1 = new RequestHeaderEnricher(headerKey1, propertyName:string.Empty, _contextAccessor);
        var clientHeaderEnricher2 = new RequestHeaderEnricher(headerKey2, propertyName:string.Empty, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher1)
            .Enrich.With(clientHeaderEnricher2)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"First testing log enricher.");
        log.Information(@"Second testing log enricher.");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(headerKey1);
        evt.Properties[headerKey1].LiteralValue().ToString().Should().Be(headerValue1);
        evt.Properties.Should().ContainKey(headerKey2.Replace("-", ""));
        evt.Properties[headerKey2.Replace("-", "")].LiteralValue().ToString().Should().Be(headerValue2);
    }

    [Test]
    public void EnrichLogWithClientHeader_WhenHttpRequestNotContainHeader_ShouldCreateHeaderValuePropertyWithNoValue()
    {
        // Arrange
        var headerKey = "RequestId";
        var clientHeaderEnricher = new RequestHeaderEnricher(headerKey, propertyName:string.Empty, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(clientHeaderEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"First testing log enricher.");
        log.Information(@"Second testing log enricher.");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(headerKey);
        evt.Properties[headerKey].LiteralValue().Should().BeNull();
    }

    [Test]
    public void WithRequestHeader_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .Enrich.WithRequestHeader("HeaderName")
            .WriteTo.Sink(new DelegatingSink(e => { }))
            .CreateLogger();

        // Act
        // Assert
        var act = () => logger.Information("LOG");
        act.Should().NotThrow();
    }

    [Test]
    public void Enrich_WhenHttpContextIsNull_DoesNotAddProperty()
    {
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns((Microsoft.AspNetCore.Http.HttpContext?)null);
        var enricher = new RequestHeaderEnricher("X-Header", "PropertyName", contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().NotContainKey("PropertyName");
    }

    [Test]
    public void Enrich_WhenPropertyNameIsNull_UsesHeaderNameWithoutHyphens()
    {
        var headerKey = "User-Agent";
        var headerValue = "TestAgent";
        _contextAccessor.HttpContext.Request.Headers[headerKey] = headerValue;

        var enricher = new RequestHeaderEnricher(headerKey, null!, _contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey("UserAgent");
        evt.Properties["UserAgent"].LiteralValue()?.ToString().Should().Be(headerValue);
    }

}
