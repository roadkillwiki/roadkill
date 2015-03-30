using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Mvc.Attributes
{
    public class ViewKeyOptionalAuthorizationAttribute : OptionalAuthorizationAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (ApplicationSettings.AllowViewKeys)
            {
                try
                {
                    var viewkey = httpContext.Request.QueryString["viewkey"];
                    if (!String.IsNullOrWhiteSpace(viewkey) && VerifyViewKey(httpContext.Request.RequestContext.RouteData, viewkey))
                        return true;
                }
                catch (Exception e)
                {
                    Log.Error("Error while checking viewkey!" + e.Message, e);
                }
            }
            return base.AuthorizeCore(httpContext);
        }

        private bool VerifyViewKey(RouteData rd, string viewkey)
        {
            if (String.IsNullOrWhiteSpace(viewkey)) return false;
            if (!rd.Values.ContainsKey("id")) return false;
            /*
            string currentAction = rd.GetRequiredString("action");
            string currentController = rd.GetRequiredString("controller");

            if (currentController != "Wiki" || currentAction != "Index") return false;
            */

            var id = int.Parse((string) rd.Values["id"]);
            return viewkey.Equals(PageService.GetViewKey(id), StringComparison.Ordinal); // retrieved from PS viewkey is null or actual viewkey
        }
    }
}
