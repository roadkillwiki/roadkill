using System.Web.Mvc;
using System.Diagnostics;
using Roadkill.Core.Configuration;
using System;
using StructureMap;
using System.Web;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// A base controller for all Roadkill controller classes which require services 
	/// (via an IServiceContainer) or authentication.
	/// </summary>
	public class ControllerBase : Controller
	{
		public ApplicationSettings ApplicationSettings { get; private set; }
		public UserManagerBase UserManager { get; private set; }
		public IUserContext Context { get; private set; }
		public SettingsManager SiteSettingsManager { get; private set; }

		public ControllerBase(ApplicationSettings settings, UserManagerBase userManager, IUserContext context, 
			SettingsManager siteSettingsManager)
		{
			ApplicationSettings = settings;
			UserManager = userManager;
			Context = context;
			SiteSettingsManager = siteSettingsManager;
		}

		protected override void OnException(ExceptionContext filterContext)
		{
			string actionName = string.Format("{0}.{1}", filterContext.RouteData.Values["controller"].ToString(), filterContext.RouteData.Values["action"].ToString());
			Log.Error("MVC error caught on {0}: {1}\n{2}", actionName, filterContext.Exception.Message, filterContext.Exception.ToString());

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
			// Redirect if Roadkill isn't installed or an upgrade is needed.
			if (!ApplicationSettings.Installed)
			{
				if (!(filterContext.Controller is InstallController))
					filterContext.Result = new RedirectResult(this.Url.Action("Index", "Install"));

				return;
			}
			else if (ApplicationSettings.UpgradeRequired)
			{
				if (!(filterContext.Controller is UpgradeController))
					filterContext.Result = new RedirectResult(this.Url.Action("Index", "Upgrade"));

				return;
			}

			Context.CurrentUser = UserManager.GetLoggedInUserName(HttpContext);
			ViewBag.Context = Context;
			ViewBag.Config = ApplicationSettings;

			// This is a fix for versions before 1.5 storing the username instead of a guid in the login cookie
			if (!ApplicationSettings.UseWindowsAuthentication)
			{
				Guid userId;
				if (!Guid.TryParse(Context.CurrentUser, out userId))
				{
					UserManager.Logout();
				}
			}
		}
	}
}
