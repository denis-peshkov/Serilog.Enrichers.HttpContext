namespace Serilog.Enrichers.HttpContext.Tests.Extensions;

internal static class Extensions
{
    public static object LiteralValue(this LogEventPropertyValue @this)
    {
        return ((ScalarValue)@this).Value;
    }
}
