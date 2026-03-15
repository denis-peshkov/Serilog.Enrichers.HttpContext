namespace Serilog.Enrichers.HttpContext.Tests.Enrichers;

public class MemoryUsageExactEnricherTests
{
    [Test]
    public void Enrich_WhenHttpContextIsNull_DoesNotAddProperty()
    {
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns((Microsoft.AspNetCore.Http.HttpContext?)null);
        var enricher = new MemoryUsageExactEnricher(contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().NotContainKey(MemoryUsageExactEnricher.PROPERTY_NAME);
    }

    [Test]
    public void Enrich_WhenItemsContainsStartMemory_AddsMemoryUsageExactProperty()
    {
        var context = new DefaultHttpContext();
        var startMemory = Process.GetCurrentProcess().WorkingSet64 - 1000;
        context.Items[MemoryUsageExactEnricher.ITEM_KEY] =
            new LogEventProperty(MemoryUsageExactEnricher.PROPERTY_NAME, new ScalarValue(startMemory));
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(context);
        var enricher = new MemoryUsageExactEnricher(contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().ContainKey(MemoryUsageExactEnricher.PROPERTY_NAME);
        var value = evt.Properties[MemoryUsageExactEnricher.PROPERTY_NAME].LiteralValue();
        value.Should().NotBeNull();
        var memory = Convert.ToInt64(value);
        memory.Should().BeGreaterThanOrEqualTo(0);
    }

    [Test]
    public void Enrich_WhenItemsDoesNotContainStartMemory_DoesNotAddProperty()
    {
        var context = new DefaultHttpContext();
        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(context);
        var enricher = new MemoryUsageExactEnricher(contextAccessor);

        LogEvent? evt = null;
        var log = new LoggerConfiguration()
            .Enrich.With(enricher)
            .WriteTo.Sink(new DelegatingSink(e => evt = e))
            .CreateLogger();

        log.Information("test");

        evt.Should().NotBeNull();
        evt!.Properties.Should().NotContainKey(MemoryUsageExactEnricher.PROPERTY_NAME);
    }

    [Test]
    public void WithMemoryUsageExact_ThenLoggerIsCalled_ShouldNotThrowException()
    {
        var logger = new LoggerConfiguration()
            .Enrich.WithMemoryUsageExact()
            .WriteTo.Sink(new DelegatingSink(_ => { }))
            .CreateLogger();
        var act = () => logger.Information("LOG");
        act.Should().NotThrow();
    }
}
