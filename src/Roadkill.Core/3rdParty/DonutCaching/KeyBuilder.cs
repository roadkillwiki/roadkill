using System.Collections.Generic;
using System.Text;
using System.Web.Routing;

namespace DevTrends.MvcDonutCaching
{
    public class KeyBuilder : IKeyBuilder
    {
        private const string CacheKeyPrefix = "_d0nutc@che.";

        public string BuildKey(string controllerName)
        {
            return BuildKey(controllerName, null, null);
        }

        public string BuildKey(string controllerName, string actionName)
        {
            return BuildKey(controllerName, actionName, null);
        }

        public string BuildKey(string controllerName, string actionName, RouteValueDictionary routeValues)
        {
            var builder = new StringBuilder(CacheKeyPrefix);

            if (controllerName != null)
            {
                builder.AppendFormat("{0}.", controllerName.ToLowerInvariant());
            }

            if (actionName != null)
            {
                builder.AppendFormat("{0}#", actionName.ToLowerInvariant());
            }

            if (routeValues != null)
            {
                foreach (var routeValue in routeValues)
                {
                    builder.Append(BuildKeyFragment(routeValue));
                }
            }

            return builder.ToString();
        }

        public string BuildKeyFragment(KeyValuePair<string, object> routeValue)
        {
            var value = routeValue.Value == null ? "<null>" : routeValue.Value.ToString().ToLowerInvariant();

            return string.Format("{0}={1}#", routeValue.Key.ToLowerInvariant(), value);
        }
    }
}
