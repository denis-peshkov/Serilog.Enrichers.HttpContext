namespace Serilog.Enrichers.HttpContext.Tests.Extensions;

public class ApplicationBuilderExtensionsTests
{
    [Fact]
    public void UseSerilogMemoryUsageExact_WhenAppIsNull_ThrowsArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            ApplicationBuilderExtensions.UseSerilogMemoryUsageExact(null!));
        Assert.Equal("app", ex.ParamName);
    }
}
