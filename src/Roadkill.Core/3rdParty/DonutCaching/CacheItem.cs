using System;

namespace DevTrends.MvcDonutCaching
{
    [Serializable]
    public class CacheItem
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
    }
}
