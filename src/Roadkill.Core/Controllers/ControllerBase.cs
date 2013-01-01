using System.Web.Mvc;
using System.Diagnostics;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// A base controller for all Roadkill controller classes which require services 
	/// (via an IServiceContainer) or authentication.
	/// </summary>
	public class ControllerBase : Controller
	{
		public IConfigurationContainer Configuration { get; private set; }
		public UserManager UserManager { get; private set; }
		public IRoadkillContext Context { get; private set; }

		public ControllerBase(IConfigurationContainer configuration, UserManager userManager, IRoadkillContext context)
		{
			Configuration = configuration;
			UserManager = userManager;
			Context = context;
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

			Context.CurrentUser = UserManager.GetLoggedInUserName(HttpContext);
			ViewBag.Context = Context;
			ViewBag.Config = Configuration;			
		}
	}
}
