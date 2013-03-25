using System;
using System.Web;
using System.Web.UI;

namespace DevTrends.MvcDonutCaching
{
    public class CacheHeadersHelper : ICacheHeadersHelper
    {
        public void SetCacheHeaders(HttpResponseBase response, CacheSettings settings)
        {
            var cacheability = HttpCacheability.NoCache;

            switch (settings.Location)
            {
                case OutputCacheLocation.Any:
                case OutputCacheLocation.Downstream:
                    cacheability = HttpCacheability.Public;
                    break;
                case OutputCacheLocation.Client:
                case OutputCacheLocation.ServerAndClient:
                    cacheability = HttpCacheability.Private;
                    break;                    
            }

            response.Cache.SetCacheability(cacheability);

            if (cacheability != HttpCacheability.NoCache)
            {
                response.Cache.SetExpires(DateTime.Now.AddSeconds(settings.Duration));
                response.Cache.SetMaxAge(new TimeSpan(0, 0, settings.Duration));
            }

            if (settings.NoStore)
            {
                response.Cache.SetNoStore();
            }
        }
    }
}
