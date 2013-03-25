using System.Web.Mvc;

namespace DevTrends.MvcDonutCaching
{
    public interface IKeyGenerator
    {
        string GenerateKey(ControllerContext context, CacheSettings cacheSettings);
    }
}
