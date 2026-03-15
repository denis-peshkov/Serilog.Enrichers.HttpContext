namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class ClientIpEnricherTests
{
    private const string ForwardHeaderKey = "x-forwarded-for";
    private readonly IHttpContextAccessor _contextAccessor;

    public ClientIpEnricherTests()
    {
        var httpContext = new DefaultHttpContext();
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _contextAccessor.HttpContext.Returns(httpContext);
    }

    [Theory]
    [InlineData("::1")]
    [InlineData("192.168.1.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
    public void EnrichLogWithClientIp_ShouldCreateClientIpPropertyWithValue(string ip)
    {
        // Arrange
        var ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = ipAddress;

        var ipEnricher = new ClientIpEnricher(ForwardHeaderKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(ipAddress.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WhenLogMoreThanOnce_ShouldReadClientIpValueFromHttpContextItems()
    {
        //Arrange
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        var ipEnricher = new ClientIpEnricher(ForwardHeaderKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");
        log.Information(@"Has an other IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Loopback.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Theory]
    [InlineData("::1")]
    [InlineData("192.168.1.1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [InlineData("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
    public void EnrichLogWithClientIp_WhenRequestContainForwardHeader_ShouldCreateClientIpPropertyWithValue(string ip)
    {
        //Arrange
        var ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        _contextAccessor.HttpContext.Request.Headers.Add(ForwardHeaderKey, ipAddress.ToString());

        var ipEnricher = new ClientIpEnricher(ForwardHeaderKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(ipAddress.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void EnrichLogWithClientIp_WithCustomForwardHeaderAndRequest_ShouldCreateClientIpPropertyWithValue()
    {
        //Arrange
        const string customForwardHeader = "CustomForwardHeader";
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        _contextAccessor.HttpContext.Request.Headers.Add(customForwardHeader, IPAddress.Broadcast.ToString());

        var ipEnricher = new ClientIpEnricher(customForwardHeader, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");

        // Assert
        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal(IPAddress.Broadcast.ToString(), evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void WithClientIp_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .Enrich.WithClientIp()
            .WriteTo.Sink(new DelegatingSink(e => { }))
            .CreateLogger();

        // Act
        var exception = Record.Exception(() => logger.Information("LOG"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void Enrich_WhenHttpContextIsNull_DoesNotAddProperty()
    {
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns((Microsoft.AspNetCore.Http.HttpContext?)null);
        var enricher = new ClientIpEnricher(ForwardHeaderKey, contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        Assert.NotNull(evt);
        Assert.False(evt.Properties.ContainsKey("ClientIp"));
    }

    [Fact]
    public void Enrich_WhenIpCannotBeDetermined_ReturnsUnknown()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = null;
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(context);
        var enricher = new ClientIpEnricher(ForwardHeaderKey, contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal("unknown", evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void Enrich_WhenForwardHeaderContainsCommaSeparatedList_UsesFirstIp()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[ForwardHeaderKey] = " 192.168.1.1 , 10.0.0.1 ";
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(context);
        var enricher = new ClientIpEnricher(ForwardHeaderKey, contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        Assert.NotNull(evt);
        Assert.True(evt.Properties.ContainsKey("ClientIp"));
        Assert.Equal("192.168.1.1", evt.Properties["ClientIp"].LiteralValue());
    }

    [Fact]
    public void ClientIpEnricher_PublicConstructor_Works()
    {
        var enricher = new ClientIpEnricher("x-forwarded-for");
        Assert.NotNull(enricher);
    }
}
