using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using System.Web;
using System.Diagnostics;
using System.Threading;
using Roadkill.Core.Domain;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// A base controller for all Roadkill controller classes which require services 
	/// (via an IServiceContainer) or authentication.
	/// </summary>
	public class ControllerBase : Controller
	{
		protected IConfigurationContainer Configuration;
		protected UserManager UserManager;

		public ControllerBase(IConfigurationContainer configuration, UserManager userManager)
		{
			Configuration = configuration;
			UserManager = userManager;
		}

		protected override void OnException(ExceptionContext filterContext)
		{
			Trace.WriteLine(filterContext.Exception, "Exception");
			base.OnException(filterContext);
		}

		/// <summary>
		/// Called before the action method is invoked. This overides the default behaviour by 
		/// populating RoadkillContext.Current.CurrentUser with the current logged in user after
		/// each action method.
		/// </summary>
		/// <param name="filterContext">Information about the current request and action.</param>
		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!Configuration.ApplicationSettings.Installed)
			{
				if (!(filterContext.Controller is InstallController))
					filterContext.Result = new RedirectResult(this.Url.Action("Index","Install"));

				return;
			}

#if APPHARBOR
			// To be removed in 1.5
			if (Request.QueryString["locale"] == "on")
			{
				Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("fi-FI");
			}
			else if (Request.QueryString["locale"] == "off")
			{
				Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
			}
#endif

			RoadkillContext.Current.CurrentUser = UserManager.GetLoggedInUserName(HttpContext);
		}
	}
}
