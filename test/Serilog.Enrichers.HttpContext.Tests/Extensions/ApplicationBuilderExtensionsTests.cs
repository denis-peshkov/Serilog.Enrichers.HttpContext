namespace Serilog.Enrichers.HttpContext.Tests.Extensions;

public class ApplicationBuilderExtensionsTests
{
    [Test]
    public void UseSerilogMemoryUsageExact_WhenAppIsNull_ThrowsArgumentNullException()
    {
        var act = () => ApplicationBuilderExtensions.UseSerilogMemoryUsageExact(null!);
        var ex = act.Should().Throw<ArgumentNullException>();
        ex.And.ParamName.Should().Be("app");
    }
}
