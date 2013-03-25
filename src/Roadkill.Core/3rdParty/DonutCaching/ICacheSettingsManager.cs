using System.Web.Configuration;

namespace DevTrends.MvcDonutCaching
{
    public interface ICacheSettingsManager
    {
        string RetrieveOutputCacheProviderType();
        OutputCacheProfile RetrieveOutputCacheProfile(string cacheProfileName);

        bool IsCachingEnabledGlobally { get; }
    }
}
