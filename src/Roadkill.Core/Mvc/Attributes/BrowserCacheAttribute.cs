using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using StructureMap;
using StructureMap.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Attachments;
using Roadkill.Core.Cache;
using Roadkill.Core.Extensions;
using Roadkill.Core.DI;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Includes 304 modified header support on the client.
	/// </summary>
	public class BrowserCacheAttribute : ActionFilterAttribute, ISetterInjected
	{
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		[SetterProperty]
		public IUserContext Context { get; set; }

		[SetterProperty]
		public UserServiceBase UserService { get; set; }

		[SetterProperty]
		public IPageService PageService { get; set; }

		[SetterProperty]
		public SettingsService SettingsService { get; set; }

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			if (!ApplicationSettings.Installed || !ApplicationSettings.UseBrowserCache || Context.IsLoggedIn)
				return;

			WikiController wikiController = filterContext.Controller as WikiController;
			HomeController homeController = filterContext.Controller as HomeController;

			if (wikiController == null && homeController == null)
				return;

			PageViewModel page = null;

			// Find the page for the action we're on
			if (wikiController != null)
			{
				int id = 0;
				if (int.TryParse(filterContext.RouteData.Values["id"].ToString(), out id))
				{
					page = PageService.GetById(id, true);
				}
			}
			else
			{
				page = PageService.FindHomePage();
			}

			if (page != null && page.IsCacheable)
			{
				string modifiedSinceHeader = filterContext.HttpContext.Request.Headers["If-Modified-Since"];
				DateTime modifiedSinceDate = ResponseWrapper.GetLastModifiedDate(modifiedSinceHeader);

				// Check if any plugins have been recently updated as saving their settings invalidates the browser cache.
				// This is necessary because, for example, enabling the TOC plugin will mean the content
				// should have {TOC} parsed now, but the browser cache content will contain the un-cached version still.
				SiteSettings siteSettings = SettingsService.GetSiteSettings();
				DateTime pluginLastSaveDate = siteSettings.PluginLastSaveDate.ClearMilliseconds();

				if (pluginLastSaveDate > modifiedSinceDate)
				{
					// Update the browser's modified since date, and a 200
					SetRequiredCacheHeaders(filterContext);
					filterContext.HttpContext.Response.Cache.SetLastModified(pluginLastSaveDate.ToUniversalTime());
					filterContext.HttpContext.Response.StatusCode = 200;

					return;
				}

				//
				// Is the page's last modified date after the plugin last save date?
				//
				DateTime pageModifiedDate = page.ModifiedOn.ToUniversalTime();

				if (pageModifiedDate > pluginLastSaveDate)
				{
					// [Yes] - check if the page's last modified date is more recent than the header. If it isn't then a 304 is returned.
					filterContext.HttpContext.Response.Cache.SetLastModified(pageModifiedDate);
					filterContext.HttpContext.Response.StatusCode = ResponseWrapper.GetStatusCodeForCache(pageModifiedDate, modifiedSinceHeader);

				}
				else
				{
					// [No]  - check if the plugin's last saved date is more recent than the header. If it isn't then a 304 is returned.
					filterContext.HttpContext.Response.Cache.SetLastModified(pluginLastSaveDate.ToUniversalTime());
					filterContext.HttpContext.Response.StatusCode = ResponseWrapper.GetStatusCodeForCache(pluginLastSaveDate, modifiedSinceHeader);
				}

				SetRequiredCacheHeaders(filterContext);

				// Lastly, if the status code is 304 then return an empty body (HttpStatusCodeResult 304), or the 
				// browser will try to read the entire response body again.
				if (filterContext.HttpContext.Response.StatusCode == 304)
				{
					filterContext.Result = new HttpStatusCodeResult(304, "Not Modified");
				}
			}
		}

		private void SetRequiredCacheHeaders(ResultExecutedContext filterContext)
		{
			// These cache headers are required for the last modified header to be understood by the browser
			filterContext.HttpContext.Response.Cache.SetCacheability(HttpCacheability.Public);
			filterContext.HttpContext.Response.Cache.SetExpires(DateTime.UtcNow.AddSeconds(2));
			filterContext.HttpContext.Response.Cache.SetMaxAge(TimeSpan.FromSeconds(0));
		}
	}
}
