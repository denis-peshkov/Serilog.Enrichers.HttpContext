[assembly: InternalsVisibleTo("Serilog.Enrichers.HttpContext.Tests")]
namespace Serilog.Enrichers
{
    public class ClientIpEnricher : ILogEventEnricher
    {
        private const string IP_ADDRESS_ITEM_KEY = "Serilog_ClientIp";
        private const string IP_ADDRESS_PROPERTY_NAME = "ClientIp";
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

            if (httpContext.Items[IP_ADDRESS_ITEM_KEY] is LogEventProperty logEventProperty)
            {
                logEvent.AddPropertyIfAbsent(logEventProperty);
                return;
            }

            var ipAddress = GetIpAddress();

            if (string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = "unknown";

            var ipAddressProperty = new LogEventProperty(IP_ADDRESS_PROPERTY_NAME, new ScalarValue(ipAddress));
            httpContext.Items.Add(IP_ADDRESS_ITEM_KEY, ipAddressProperty);

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
