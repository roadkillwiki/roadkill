using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using StructureMap;
using StructureMap.Attributes;

namespace Roadkill.Core
{
	/// <summary>
	/// Includes 304 modified header support on the client.
	/// </summary>
	public class BrowserCacheAttribute : ActionFilterAttribute, IInjectedAttribute
	{
		[SetterProperty]
		public IConfigurationContainer Configuration { get; set; }

		[SetterProperty]
		public IRoadkillContext Context { get; set; }

		[SetterProperty]
		public UserManager UserManager { get; set; }

		[SetterProperty]
		public PageManager PageManager { get; set; }

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			if (!Configuration.ApplicationSettings.Installed || !Configuration.ApplicationSettings.UseBrowserCache)
				return;

			if (Configuration.ApplicationSettings.UseBrowserCache || Context.IsLoggedIn)
				return;

			WikiController wikiController = filterContext.Controller as WikiController;
			HomeController homeController = filterContext.Controller as HomeController;

			if (wikiController == null && homeController == null)
				return;

			PageSummary summary = null;

			// Find the page for the action we're on
			if (wikiController != null)
			{
				int id = 0;
				if (int.TryParse(filterContext.RouteData.Values["id"].ToString(), out id))
				{
					summary = PageManager.GetById(id);
				}
			}
			else
			{
				summary = PageManager.FindHomePage();
			}

			if (summary != null)
			{
				filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
				filterContext.HttpContext.Response.Cache.SetExpires(DateTime.Now.AddSeconds(2));
				filterContext.HttpContext.Response.Cache.SetMaxAge(TimeSpan.FromSeconds(0));
				filterContext.HttpContext.Response.Cache.SetLastModified(summary.ModifiedOn.ToUniversalTime());
				filterContext.HttpContext.Response.StatusCode = filterContext.HttpContext.ApplicationInstance.Context.GetStatusCodeForCache(summary.ModifiedOn.ToUniversalTime());

				if (filterContext.HttpContext.Response.StatusCode == 304)
				{
					filterContext.Result = new HttpStatusCodeResult(304, "Not Modified");
				}
			}
		}
	}
}
