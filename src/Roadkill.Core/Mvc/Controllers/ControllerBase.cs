using System.Web.Mvc;
using System.Diagnostics;
using Roadkill.Core.Configuration;
using System;
using StructureMap;
using System.Web;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Logging;
using System.Collections.Generic;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// A base controller for all Roadkill controller classes which require services 
	/// (via an IServiceContainer) or authentication.
	/// </summary>
	public class ControllerBase : Controller
	{
		public ApplicationSettings ApplicationSettings { get; private set; }
		public UserServiceBase UserService { get; private set; }
		public IUserContext Context { get; private set; }
		public SettingsService SettingsService { get; private set; }

		public ControllerBase(ApplicationSettings settings, UserServiceBase userService, IUserContext context, 
			SettingsService settingsService)
		{
			ApplicationSettings = settings;
			UserService = userService;
			Context = context;
			SettingsService = settingsService;
		}

		protected override void OnException(ExceptionContext filterContext)
		{
			// Log the route data values
			List<string> routeData = new List<string>();
			foreach (string key in filterContext.RouteData.Values.Keys)
			{
				routeData.Add(string.Format("'{0}' : '{1}'", key, filterContext.RouteData.Values[key]));
			}

			string routeInfo = string.Join(", ", routeData);
			Log.Error("MVC error caught. Route data: [{0}] - {1}\n{2}", routeInfo, filterContext.Exception.Message, filterContext.Exception.ToString());

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

			Context.CurrentUser = UserService.GetLoggedInUserName(HttpContext);
			ViewBag.Context = Context;
			ViewBag.Config = ApplicationSettings;
		}
	}
}
