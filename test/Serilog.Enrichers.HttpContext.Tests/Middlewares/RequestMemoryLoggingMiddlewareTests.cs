namespace Serilog.Enrichers.HttpContext.Tests.Middlewares;

using Serilog.Middlewares;

public class RequestMemoryLoggingMiddlewareTests
{
    [Test]
    public void Constructor_WhenNextIsNull_ThrowsArgumentNullException()
    {
        var act = () => new RequestMemoryLoggingMiddleware(null!);
        act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("next");
    }

    [Test]
    public void Invoke_WhenHttpContextIsNull_ThrowsArgumentNullException()
    {
        var middleware = new RequestMemoryLoggingMiddleware(_ => Task.CompletedTask);
        var act = () => middleware.Invoke(null!);
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Invoke_StoresStartMemoryInHttpContextItems()
    {
        Microsoft.AspNetCore.Http.HttpContext capturedContext = null!;
        var middleware = new RequestMemoryLoggingMiddleware(ctx =>
        {
            capturedContext = ctx;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        await middleware.Invoke(context);
        capturedContext.Should().NotBeNull();
        context.Items.Should().ContainKey(MemoryUsageExactEnricher.ITEM_KEY);
        var prop = context.Items[MemoryUsageExactEnricher.ITEM_KEY] as LogEventProperty;
        prop.Should().NotBeNull();
        prop!.Name.Should().Be(MemoryUsageExactEnricher.PROPERTY_NAME);
    }
}
