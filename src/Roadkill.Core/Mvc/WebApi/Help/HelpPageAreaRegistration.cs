using System.Web.Http;
using System.Web.Mvc;

namespace Roadkill.Core.Areas.HelpPage
{
	public class HelpPageAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "HelpPage";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
			// "/api"
			context.MapRoute(
				"HelpPage_Default",
				"api",
				new { controller = "Help", action = "Index", apiId = UrlParameter.Optional });

			// "/api/help"
			context.MapRoute(
				"HelpPage_Default2",
				"api/help",
				new { controller = "Help", action = "Index", apiId = UrlParameter.Optional },
				null,
				new string[] { "Roadkill.Core.Areas.HelpPage.Controllers" }
			);

			// "/api/help/pages"
			context.MapRoute(
				"HelpPage_Default3",
				"api/help/{apiId}/{action}/",
				new { controller = "Help", action = "Index", apiId = UrlParameter.Optional },
				null,
				new string[] { "Roadkill.Core.Areas.HelpPage.Controllers" }
			);

			HelpPageConfig.Register(GlobalConfiguration.Configuration);
        }
    }
}