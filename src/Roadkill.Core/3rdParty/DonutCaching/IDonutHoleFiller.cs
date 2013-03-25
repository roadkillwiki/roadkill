using System.Web.Mvc;

namespace DevTrends.MvcDonutCaching
{
    public interface IDonutHoleFiller
    {
        string RemoveDonutHoleWrappers(string content, ControllerContext filterContext);
        string ReplaceDonutHoleContent(string content, ControllerContext filterContext);
    }
}
