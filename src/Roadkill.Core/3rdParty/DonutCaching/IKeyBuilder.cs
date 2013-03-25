using System.Collections.Generic;
using System.Web.Routing;

namespace DevTrends.MvcDonutCaching
{
    public interface IKeyBuilder
    {
        string BuildKey(string controllerName);
        string BuildKey(string controllerName, string actionName);
        string BuildKey(string controllerName, string actionName, RouteValueDictionary routeValues);
        string BuildKeyFragment(KeyValuePair<string, object> routeValue);
    }
}
