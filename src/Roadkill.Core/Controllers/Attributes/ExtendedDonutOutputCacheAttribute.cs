using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DevTrends.MvcDonutCaching;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using StructureMap;

namespace Roadkill.Core
{
	/// <summary>
	/// Extends the DonutOutputCache to include 304 modified header support on the client.
	/// </summary>
	public class ExtendedDonutOutputCacheAttribute : DonutOutputCacheAttribute
	{
		public ExtendedDonutOutputCacheAttribute() : base()
		{
			CacheHeadersHelper = new DummyCacheHeadersHelper();
		}

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			// TODO: add IPageController interface implementation for both controllers
			PageManager manager = null;
			IConfigurationContainer config = null;
			IRoadkillContext context = null;
			PageSummary summary = null;

			WikiController wikiController = filterContext.Controller as WikiController;
			if (wikiController != null)
			{
				manager = wikiController.PageManager;
				config = wikiController.Configuration;
				context = wikiController.Context;

				if (!config.ApplicationSettings.Installed)
					return;

				int id = 0;
				if (int.TryParse(filterContext.RouteData.Values["id"].ToString(), out id))
				{
					summary = manager.GetById(id);
				}
			}
			else
			{
				HomeController homeController = filterContext.Controller as HomeController;
				if (homeController != null)
				{
					manager = homeController.PageManager;
					config = homeController.Configuration;

					if (!config.ApplicationSettings.Installed)
						return;

					context = homeController.Context;
					summary = manager.FindHomePage();
				}
			}

			// Nuts
			base.OnResultExecuted(filterContext);

			if (manager != null)
			{
				if (config.ApplicationSettings.UseBrowserCache && !context.IsLoggedIn)
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

		private class DummyCacheHeadersHelper : ICacheHeadersHelper
		{
			public void SetCacheHeaders(HttpResponseBase response, CacheSettings settings) { }
		}
	}
}
