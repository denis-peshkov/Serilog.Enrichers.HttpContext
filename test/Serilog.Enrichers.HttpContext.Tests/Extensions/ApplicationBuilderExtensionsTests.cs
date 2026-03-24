namespace Serilog.Enrichers.HttpContext.Tests.Extensions;

#if NET6_0_OR_GREATER
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
#endif

public class ApplicationBuilderExtensionsTests
{
    [Test]
    public void UseSerilogMemoryUsageExact_WhenAppIsNull_ThrowsArgumentNullException()
    {
        var act = () => Serilog.Extensions.ApplicationBuilderExtensions.UseSerilogMemoryUsageExact(null!);
        var ex = act.Should().Throw<ArgumentNullException>();
        ex.And.ParamName.Should().Be("app");
    }

#if NET6_0_OR_GREATER
    [Test]
    public void UseSerilogMemoryUsageExact_WhenAppIsValid_ReturnsSameBuilder()
    {
        var services = new ServiceCollection();
        var sp = services.BuildServiceProvider();
        var appBuilder = new ApplicationBuilder(sp);
        var result = Serilog.Extensions.ApplicationBuilderExtensions.UseSerilogMemoryUsageExact(appBuilder);
        result.Should().BeSameAs(appBuilder);
    }
#endif
}
