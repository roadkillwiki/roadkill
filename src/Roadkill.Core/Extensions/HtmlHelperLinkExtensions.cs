using System;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Localization;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Core.Extensions
{
	public static class HtmlHelperLinkExtensions
	{
		private static IUserContext GetUserContext(IHtmlHelper helper)
		{
			return helper.ViewContext.HttpContext.RequestServices.GetService<IUserContext>();
		}

		public static IHtmlContent SettingsLink(this IHtmlHelper helper, string prefix, string suffix)
		{
			IUserContext context = GetUserContext(helper);
			if (context != null && context.IsAdmin)
			{
				string link = helper.ActionLink(SiteStrings.Navigation_SiteSettings, "Index", "Settings", null, null, null, new { area = "SiteSettings" }, null).ToHtmlString();
				return new HtmlString(prefix + link + suffix);
			}

			return HtmlString.Empty;
		}

		public static IHtmlContent FileManagerLink(this IHtmlHelper helper, string prefix, string suffix)
		{
			IUserContext context = GetUserContext(helper);
			if (context != null && (context.IsLoggedIn && (context.IsAdmin || context.IsEditor)))
			{
				string link = helper.ActionLink(SiteStrings.FileManager_Title, "Index", "FileManager", null, null, null, new { area = "" }, null).ToHtmlString();
				return new HtmlString(prefix + link + suffix);
			}

			return HtmlString.Empty;
		}

		public static IHtmlContent LoginLink(this IHtmlHelper helper, string prefix, string suffix)
		{
			IUserContext context = GetUserContext(helper);
			if (context == null)
				return HtmlString.Empty;

			string link = "";

			if (context.IsLoggedIn)
			{
				link = helper.ActionLink(SiteStrings.Navigation_Logout, "Logout", "User", null, null, null, new { area = "" }, null).ToHtmlString();
			}
			else
			{
				string redirectPath = helper.ViewContext.HttpContext.Request.Path;
				link = helper.ActionLink(SiteStrings.Navigation_Login, "Login", "User", null, null, null, new { ReturnUrl = redirectPath, area = "" }, null).ToHtmlString();

				SettingsService settingsService = helper.ViewContext.HttpContext.RequestServices.GetService<SettingsService>();
				if (settingsService != null && settingsService.GetSiteSettings().AllowUserSignup)
					link += "&nbsp;/&nbsp;" + helper.ActionLink(SiteStrings.Navigation_Register, "Signup", "User", null, null, null, new { area = "" }, null).ToHtmlString();
			}

			return new HtmlString(prefix + link + suffix);
		}

		public static IHtmlContent NewPageLink(this IHtmlHelper helper, string prefix, string suffix)
		{
			IUserContext context = GetUserContext(helper);
			if (context != null && (context.IsLoggedIn && (context.IsAdmin || context.IsEditor)))
			{
				string link = helper.ActionLink(SiteStrings.Navigation_NewPage, "New", "Pages", null, null, null, new { area = "" }, null).ToHtmlString();
				return new HtmlString(prefix + link + suffix);
			}

			return HtmlString.Empty;
		}

		public static IHtmlContent MainPageLink(this IHtmlHelper helper, string linkText, string prefix, string suffix)
		{
			string link = helper.ActionLink(linkText, "Index", "Home", null, null, null, new { area = "" }, null).ToHtmlString();
			return new HtmlString(prefix + link + suffix);
		}

		public static IHtmlContent PageLink(this IHtmlHelper helper, string linkText, string pageTitle)
		{
			return helper.PageLink(linkText, pageTitle, null, "", "");
		}

		public static IHtmlContent PageLink(this IHtmlHelper helper, string linkText, string pageTitle, string prefix, string suffix)
		{
			return helper.PageLink(linkText, pageTitle, null, prefix, suffix);
		}

		public static IHtmlContent PageLink(this IHtmlHelper helper, string linkText, string pageTitle, object htmlAttributes, string prefix, string suffix, IPageService pageService = null)
		{
			if (pageService == null)
				pageService = helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IPageService>();

			PageViewModel model = pageService.FindByTitle(pageTitle);
			if (model != null)
			{
				string link = helper.ActionLink(linkText, "Index", "Wiki", null, null, null, new { id = model.Id, title = pageTitle, area = "" }, htmlAttributes).ToHtmlString();
				return new HtmlString(prefix + link + suffix);
			}

			return new HtmlString(linkText);
		}
	}
}
