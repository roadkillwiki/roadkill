using System;
using System.Web.Http;
using System.Web.Mvc;
using Roadkill.Core.Areas.HelpPage.Models;
using Roadkill.Core.Configuration;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Areas.HelpPage.Controllers
{
    /// <summary>
    /// The controller that will handle requests for the help page.
    /// </summary>
	public class HelpController : Roadkill.Core.Mvc.Controllers.ControllerBase
	{
		public HelpController(ApplicationSettings settings, UserServiceBase userManager, IUserContext context,
			SettingsService settingsService)
			: base(settings, userManager, context, settingsService)
		{
			Configuration = GlobalConfiguration.Configuration;
		}

        public HttpConfiguration Configuration { get; private set; }

        public ActionResult Index()
        {
            ViewBag.DocumentationProvider = Configuration.Services.GetDocumentationProvider();
            return View(Configuration.Services.GetApiExplorer().ApiDescriptions);
        }

        public ActionResult Api(string apiId)
        {
            if (!String.IsNullOrEmpty(apiId))
            {
                HelpPageApiModel apiModel = Configuration.GetHelpPageApiModel(apiId);
                if (apiModel != null)
                {
                    return View(apiModel);
                }
            }

            return View("Error");
        }
    }
}