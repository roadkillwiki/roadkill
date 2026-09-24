using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Extensions;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Sends the Last-Modified browser cache headers (and a 304 when the page hasn't changed) for wiki pages and the home page,
	/// for users that aren't logged in.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class BrowserCacheAttribute : Attribute, IResultFilter
	{
		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			IServiceProvider services = filterContext.HttpContext.RequestServices;
			ApplicationSettings applicationSettings = services.GetRequiredService<ApplicationSettings>();
			IUserContext userContext = services.GetRequiredService<IUserContext>();

			if (!applicationSettings.Installed || !applicationSettings.UseBrowserCache || userContext.IsLoggedIn)
				return;

			WikiController wikiController = filterContext.Controller as WikiController;
			HomeController homeController = filterContext.Controller as HomeController;

			if (wikiController == null && homeController == null)
				return;

			IPageService pageService = services.GetRequiredService<IPageService>();
			PageViewModel page = null;

			// Find the page for the action we're on
			if (wikiController != null)
			{
				int id = 0;
				object idValue = filterContext.RouteData.Values["id"];
				if (idValue != null && int.TryParse(idValue.ToString(), out id))
				{
					page = pageService.GetById(id, true);
				}
			}
			else
			{
				page = pageService.FindHomePage();
			}

			if (page == null || !page.IsCacheable)
				return;

			HttpResponse response = filterContext.HttpContext.Response;
			string modifiedSinceHeader = filterContext.HttpContext.Request.Headers[HeaderNames.IfModifiedSince];
			DateTime modifiedSinceDate = ResponseWrapper.GetLastModifiedDate(modifiedSinceHeader);

			// Check if any plugins have been recently updated as saving their settings invalidates the browser cache.
			// This is necessary because, for example, enabling the TOC plugin will mean the content
			// should have {TOC} parsed now, but the browser cache content will contain the un-cached version still.
			SiteSettings siteSettings = services.GetRequiredService<SettingsService>().GetSiteSettings();
			DateTime pluginLastSaveDate = siteSettings.PluginLastSaveDate.ClearMilliseconds();

			if (pluginLastSaveDate > modifiedSinceDate)
			{
				// Update the browser's modified since date, and a 200
				SetRequiredCacheHeaders(response);
				SetLastModified(response, pluginLastSaveDate.ToUniversalTime());
				response.StatusCode = 200;

				return;
			}

			int statusCode;
			DateTime pageModifiedDate = page.ModifiedOn.ToUniversalTime();

			// Is the page's last modified date after the plugin last save date?
			if (pageModifiedDate > pluginLastSaveDate)
			{
				// [Yes] - check if the page's last modified date is more recent than the header. If it isn't then a 304 is returned.
				SetLastModified(response, pageModifiedDate);
				statusCode = ResponseWrapper.GetStatusCodeForCache(pageModifiedDate, modifiedSinceHeader);
			}
			else
			{
				// [No]  - check if the plugin's last saved date is more recent than the header. If it isn't then a 304 is returned.
				SetLastModified(response, pluginLastSaveDate.ToUniversalTime());
				statusCode = ResponseWrapper.GetStatusCodeForCache(pluginLastSaveDate, modifiedSinceHeader);
			}

			SetRequiredCacheHeaders(response);

			// If the status code is 304 then return an empty body, or the browser will try to read the entire response body again.
			if (statusCode == 304)
				filterContext.Result = new StatusCodeResult(304);
			else
				response.StatusCode = statusCode;
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}

		private static void SetLastModified(HttpResponse response, DateTime lastModifiedUtc)
		{
			response.Headers[HeaderNames.LastModified] = lastModifiedUtc.ToString("R", CultureInfo.InvariantCulture);
		}

		private static void SetRequiredCacheHeaders(HttpResponse response)
		{
			// These cache headers are required for the last modified header to be understood by the browser
			response.Headers[HeaderNames.CacheControl] = "public, max-age=0";
			response.Headers[HeaderNames.Expires] = DateTime.UtcNow.AddSeconds(2).ToString("R", CultureInfo.InvariantCulture);
		}
	}
}
