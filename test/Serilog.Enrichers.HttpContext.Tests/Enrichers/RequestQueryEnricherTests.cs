using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Serilog.Enrichers.ClientInfo.Tests;

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
}
