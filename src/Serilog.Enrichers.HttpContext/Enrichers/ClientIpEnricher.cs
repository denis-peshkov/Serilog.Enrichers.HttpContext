[assembly: InternalsVisibleTo("Serilog.Enrichers.HttpContext.Tests")]
namespace Serilog.Enrichers
{
    public class ClientIpEnricher : ILogEventEnricher
    {
        private const string ITEM_KEY = $"Serilog_{PROPERTY_NAME}";
        private const string PROPERTY_NAME = "ClientIp";
        private readonly string _forwardHeaderKey;

        private readonly IHttpContextAccessor _contextAccessor;

        public ClientIpEnricher(string forwardHeaderKey)
            : this(forwardHeaderKey, new HttpContextAccessor())
        {
        }

        internal ClientIpEnricher(string forwardHeaderKey, IHttpContextAccessor contextAccessor)
        {
            _forwardHeaderKey = forwardHeaderKey;
            _contextAccessor = contextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext == null)
                return;

            if (httpContext.Items[ITEM_KEY] is LogEventProperty logEventProperty)
            {
                logEvent.AddPropertyIfAbsent(logEventProperty);
                return;
            }

            var ipAddress = GetIpAddress();

            if (string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = "unknown";

            var ipAddressProperty = new LogEventProperty(PROPERTY_NAME, new ScalarValue(ipAddress));
            httpContext.Items.Add(ITEM_KEY, ipAddressProperty);

            logEvent.AddPropertyIfAbsent(ipAddressProperty);
        }

#if NETFULL
        private string GetIpAddress()
        {
            var ipAddress = _contextAccessor.HttpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            return !string.IsNullOrEmpty(ipAddress)
                ? GetIpAddressFromProxy(ipAddress)
                : _contextAccessor.HttpContext.Request.ServerVariables["REMOTE_ADDR"];
        }
#else
        private string GetIpAddress()
        {
            var ipAddress = _contextAccessor.HttpContext?.Request?.Headers[_forwardHeaderKey].FirstOrDefault();

            return !string.IsNullOrEmpty(ipAddress)
                ? GetIpAddressFromProxy(ipAddress)
                : _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }
#endif

        private string GetIpAddressFromProxy(string proxyFieldIpList)
        {
            var addresses = proxyFieldIpList.Split(',');

            return addresses.Length == 0 ? string.Empty : addresses[0].Trim();
        }
    }
}
