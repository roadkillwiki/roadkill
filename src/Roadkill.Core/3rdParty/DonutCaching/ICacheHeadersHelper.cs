using System.Web;

namespace DevTrends.MvcDonutCaching
{
    public interface ICacheHeadersHelper
    {
        void SetCacheHeaders(HttpResponseBase response, CacheSettings settings);
    }
}
