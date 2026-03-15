namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class ClientIpEnricherTests
{
    private const string ForwardHeaderKey = "x-forwarded-for";
    private IHttpContextAccessor _contextAccessor;

    [SetUp]
    public void SetUp()
    {
        var httpContext = new DefaultHttpContext();
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(httpContext);
        _contextAccessor = mock.Object;
    }

    [TestCase("::1")]
    [TestCase("192.168.1.1")]
    [TestCase("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [TestCase("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
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
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey("ClientIp");
        evt.Properties["ClientIp"].LiteralValue().Should().Be(ipAddress.ToString());
    }

    [Test]
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
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey("ClientIp");
        evt.Properties["ClientIp"].LiteralValue().Should().Be(IPAddress.Loopback.ToString());
    }

    [TestCase("::1")]
    [TestCase("192.168.1.1")]
    [TestCase("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    [TestCase("2001:db8:85a3:8d3:1319:8a2e:370:7348")]
    public void EnrichLogWithClientIp_WhenRequestContainForwardHeader_ShouldCreateClientIpPropertyWithValue(string ip)
    {
        //Arrange
        var ipAddress = IPAddress.Parse(ip);
        _contextAccessor.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        _contextAccessor.HttpContext.Request.Headers[ForwardHeaderKey] = ipAddress.ToString();

        var ipEnricher = new ClientIpEnricher(ForwardHeaderKey, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey("ClientIp");
        evt.Properties["ClientIp"].LiteralValue().Should().Be(ipAddress.ToString());
    }

    [Test]
    public void EnrichLogWithClientIp_WithCustomForwardHeaderAndRequest_ShouldCreateClientIpPropertyWithValue()
    {
        //Arrange
        const string customForwardHeader = "CustomForwardHeader";
        _contextAccessor.HttpContext!.Connection.RemoteIpAddress = IPAddress.Loopback;
        _contextAccessor.HttpContext.Request.Headers[customForwardHeader] = IPAddress.Broadcast.ToString();

        var ipEnricher = new ClientIpEnricher(customForwardHeader, _contextAccessor);

        LogEvent evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(ipEnricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        // Act
        log.Information(@"Has an IP property");

        // Assert
        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey("ClientIp");
        evt.Properties["ClientIp"].LiteralValue().Should().Be(IPAddress.Broadcast.ToString());
    }

    [Test]
    public void WithClientIp_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        // Arrange
        var logger = new LoggerConfiguration()
            .Enrich.WithClientIp()
            .WriteTo.Sink(new DelegatingSink(e => { }))
            .CreateLogger();

        // Act & Assert
        var act = () => logger.Information("LOG");
        act.Should().NotThrow();
    }

    [Test]
    public void Enrich_WhenHttpContextIsNull_DoesNotAddProperty()
    {
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns((Microsoft.AspNetCore.Http.HttpContext?)null);
        var contextAccessor = mock.Object;
        var enricher = new ClientIpEnricher(ForwardHeaderKey, contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().NotContainKey("ClientIp");
    }

    [Test]
    public void Enrich_WhenIpCannotBeDetermined_ReturnsUnknown()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = null;
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(context);
        var contextAccessor = mock.Object;
        var enricher = new ClientIpEnricher(ForwardHeaderKey, contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey("ClientIp");
        evt.Properties["ClientIp"].LiteralValue().Should().Be("unknown");
    }

    [Test]
    public void Enrich_WhenForwardHeaderContainsCommaSeparatedList_UsesFirstIp()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[ForwardHeaderKey] = " 192.168.1.1 , 10.0.0.1 ";
        var mock = new Mock<IHttpContextAccessor>();
        mock.Setup(x => x.HttpContext).Returns(context);
        var contextAccessor = mock.Object;
        var enricher = new ClientIpEnricher(ForwardHeaderKey, contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey("ClientIp");
        evt.Properties["ClientIp"].LiteralValue().Should().Be("192.168.1.1");
    }

    [Test]
    public void ClientIpEnricher_PublicConstructor_Works()
    {
        var enricher = new ClientIpEnricher("x-forwarded-for");
        enricher.Should().NotBeNull();
    }
}
