using System;

namespace DevTrends.MvcDonutCaching
{
    internal interface IExtendedOutputCacheManager : IOutputCacheManager
    {
        void AddItem(string key, CacheItem cacheItem, DateTime utcExpiry);
        CacheItem GetItem(string key);
    }
}
