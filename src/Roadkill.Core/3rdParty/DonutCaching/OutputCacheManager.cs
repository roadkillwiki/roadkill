using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using System.Web.Routing;

namespace DevTrends.MvcDonutCaching
{
    public class OutputCacheManager : IExtendedOutputCacheManager
    {
        private readonly OutputCacheProvider _outputCacheProvider;
        private readonly IKeyBuilder _keyBuilder;

        public OutputCacheManager()
        {
            _outputCacheProvider = OutputCache.Instance;
            _keyBuilder = new KeyBuilder();
        }

        internal OutputCacheManager(OutputCacheProvider outputCacheProvider, IKeyBuilder keyBuilder)
        {
            _outputCacheProvider = outputCacheProvider;
            _keyBuilder = keyBuilder;            
        }

        public void AddItem(string key, CacheItem cacheItem, DateTime utcExpiry)
        {
            _outputCacheProvider.Add(key, cacheItem, utcExpiry);
        }

        public CacheItem GetItem(string key)
        {
            return _outputCacheProvider.Get(key) as CacheItem;
        }

        /// <summary>
        /// Removes a single output cache entry for the specified controller and action.
        /// </summary>
        /// <param name="controllerName">The name of the controller that contains the action method.</param>
        /// <param name="actionName">The name of the controller action method.</param>
        public void RemoveItem(string controllerName, string actionName)
        {
            RemoveItem(controllerName, actionName, null);
        }

        /// <summary>
        /// Removes a single output cache entry for the specified controller, action and parameters.
        /// </summary>
        /// <param name="controllerName">The name of the controller that contains the action method.</param>
        /// <param name="actionName">The name of the controller action method.</param>
        /// <param name="routeValues">An object that contains the parameters for a route.</param>
        public void RemoveItem(string controllerName, string actionName, object routeValues)
        {
            RemoveItem(controllerName, actionName, new RouteValueDictionary(routeValues));
        }

        /// <summary>
        /// Removes a single output cache entry for the specified controller, action and parameters.
        /// </summary>
        /// <param name="controllerName">The name of the controller that contains the action method.</param>
        /// <param name="actionName">The name of the controller action method.</param>
        /// <param name="routeValues">A dictionary that contains the parameters for a route.</param>
        public void RemoveItem(string controllerName, string actionName, RouteValueDictionary routeValues)
        {
            var key = _keyBuilder.BuildKey(controllerName, actionName, routeValues);

            _outputCacheProvider.Remove(key);
        }

        /// <summary>
        /// Removes all output cache entries.
        /// </summary>
        public void RemoveItems()
        {
            RemoveItems(null, null, null);
        }

        /// <summary>
        /// Removes all output cache entries for the specified controller.
        /// </summary>
        /// <param name="controllerName">The name of the controller.</param>
        public void RemoveItems(string controllerName)
        {
            RemoveItems(controllerName, null, null);
        }

        /// <summary>
        /// Removes all output cache entries for the specified controller and action.
        /// </summary>
        /// <param name="controllerName">The name of the controller that contains the action method.</param>
        /// <param name="actionName">The name of the controller action method.</param>
        public void RemoveItems(string controllerName, string actionName)
        {
            RemoveItems(controllerName, actionName, null);
        }

        /// <summary>
        /// Removes all output cache entries for the specified controller, action and parameters.
        /// </summary>
        /// <param name="controllerName">The name of the controller that contains the action method.</param>
        /// <param name="actionName">The name of the controller action method.</param>
        /// <param name="routeValues">A dictionary that contains the parameters for a route.</param>
        public void RemoveItems(string controllerName, string actionName, RouteValueDictionary routeValues)
        {
            var enumerableCache = _outputCacheProvider as IEnumerable<KeyValuePair<string, object>>;

            if (enumerableCache == null)
            {
                throw new NotSupportedException("Ensure that your custom OutputCacheProvider implements IEnumerable<KeyValuePair<string, object>>.");
            }

            var key = _keyBuilder.BuildKey(controllerName, actionName);

            var keysToDelete = enumerableCache.Where(x => x.Key.StartsWith(key)).Select(x => x.Key);

            if (routeValues != null)
            {
                foreach (var routeValue in routeValues)
                {
                    var value = routeValue;
                    keysToDelete = keysToDelete.Where(x => x.Contains(_keyBuilder.BuildKeyFragment(value)));
                }
            }

            foreach (var keyToDelete in keysToDelete)
            {
                _outputCacheProvider.Remove(keyToDelete);
            }
        }
    }
}
