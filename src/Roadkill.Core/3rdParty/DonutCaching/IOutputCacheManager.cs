using System.Web.Routing;

namespace DevTrends.MvcDonutCaching
{
    public interface IOutputCacheManager
    {
        void RemoveItem(string controllerName, string actionName);
        void RemoveItem(string controllerName, string actionName, object routeValues);
        void RemoveItem(string controllerName, string actionName, RouteValueDictionary routeValues);
        void RemoveItems();
        void RemoveItems(string controllerName);
        void RemoveItems(string controllerName, string actionName);
        void RemoveItems(string controllerName, string actionName, RouteValueDictionary routeValues);
    }
}
