using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// A base controller for all Roadkill controller classes which require services or authentication.
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

		/// <summary>
		/// Called before the action method is invoked. This populates the <see cref="IUserContext.CurrentUser"/> with 
		/// the current logged in user, and redirects to the installer if Roadkill isn't installed.
		/// </summary>
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			// Redirect if Roadkill isn't installed
			if (!ApplicationSettings.Installed)
			{
				if (!(filterContext.Controller is InstallController))
					filterContext.Result = new RedirectResult(Url.Action("Index", "Install", new { area = "" }) ?? "/install");

				return;
			}

			Context.CurrentUser = UserService.GetLoggedInUserName(HttpContext);
			ViewBag.Context = Context;
			ViewBag.Config = ApplicationSettings;
		}

		/// <summary>
		/// Logs any unhandled exceptions from the action, including the route data values.
		/// </summary>
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext.Exception != null && !filterContext.ExceptionHandled)
			{
				List<string> routeData = new List<string>();
				foreach (KeyValuePair<string, object> item in filterContext.RouteData.Values)
				{
					routeData.Add(string.Format("'{0}' : '{1}'", item.Key, item.Value));
				}

				string routeInfo = string.Join(", ", routeData);
				Log.Error("MVC error caught. Route data: [{0}] - {1}\n{2}", routeInfo, filterContext.Exception.Message, filterContext.Exception.ToString());
			}

			base.OnActionExecuted(filterContext);
		}
	}
}
