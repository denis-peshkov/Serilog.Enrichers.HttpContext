namespace Serilog.Enrichers.HttpContext.Tests.Middlewares;

using Serilog.Middlewares;

public class RequestMemoryLoggingMiddlewareTests
{
    [Fact]
    public void Constructor_WhenNextIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new RequestMemoryLoggingMiddleware(null!));
        Assert.Equal("next", ex.ParamName);
    }

    [Fact]
    public async Task Invoke_WhenHttpContextIsNull_ThrowsArgumentNullException()
    {
        var middleware = new RequestMemoryLoggingMiddleware(_ => Task.CompletedTask);
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            middleware.Invoke(null!));
    }

    [Fact]
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
        Assert.NotNull(capturedContext);
        Assert.True(context.Items.ContainsKey(MemoryUsageExactEnricher.ITEM_KEY));
        var prop = context.Items[MemoryUsageExactEnricher.ITEM_KEY] as LogEventProperty;
        Assert.NotNull(prop);
        Assert.Equal(MemoryUsageExactEnricher.PROPERTY_NAME, prop.Name);
    }
}
