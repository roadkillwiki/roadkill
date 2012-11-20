using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Roadkill.Core.Localization.Resx;
using System.Globalization;
using StructureMap;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// A set of extension methods for common links throughout the site.
	/// </summary>
	public static class HtmlLinkExtensions
	{
		/// <summary>
		/// Gets a string to indicate whether the current user is logged in.
		/// </summary>
		/// <returns>"Logged in as {user}" if the user is logged in; "Not logged in" if the user is not logged in.</returns>
		public static MvcHtmlString LoginStatus(this HtmlHelper helper)
		{
			if (RoadkillContext.Current.IsLoggedIn)
			{
				string text = string.Format("{0} {1}", SiteStrings.Shared_LoggedInAs, RoadkillContext.Current.CurrentUsername);
				return helper.ActionLink(text, "Profile", "User");
			}
			else
			{
				return MvcHtmlString.Create(SiteStrings.Shared_NotLoggedIn);
			}
		}

		/// <summary>
		/// Provides a link to the settings page, with optional prefix and suffix tags or seperators.
		/// </summary>
		/// <returns>If the user is not logged in and not an admin, an empty string is returned.</returns>
		public static MvcHtmlString SettingsLink(this HtmlHelper helper, string prefix,string suffix)
		{
			if (RoadkillContext.Current.IsAdmin)
			{
				string link = helper.ActionLink(SiteStrings.Navigation_SiteSettings, "Index", "Settings").ToString();
				return MvcHtmlString.Create(prefix + link + suffix);
			}
			else
			{
				return MvcHtmlString.Empty;
			}
		}

		/// <summary>
		/// Provides a link to the login page, or if the user is logged in, the logout page.
		/// Optional prefix and suffix tags or seperators and also included.
		/// </summary>
		/// <returns>If windows authentication is being used, an empty string is returned.</returns>
		public static MvcHtmlString LoginLink(this HtmlHelper helper, string prefix, string suffix)
		{
			if (RoadkillSettings.Current.ApplicationSettings.UseWindowsAuthentication)
				return MvcHtmlString.Empty;

			string link = "";

			if (RoadkillContext.Current.IsLoggedIn)
			{
				link = helper.ActionLink(SiteStrings.Navigation_Logout, "Logout", "User").ToString();
			}
			else
			{
				link = helper.ActionLink(SiteStrings.Navigation_Login, "Login", "User").ToString();

				if (RoadkillSettings.Current.SitePreferences.AllowUserSignup)
					link += "&nbsp;/&nbsp;" + helper.ActionLink(SiteStrings.Navigation_Register, "Signup", "User").ToString();
			}

			return MvcHtmlString.Create(prefix + link + suffix);
		}

		/// <summary>
		/// Provides a link to the "new page" page, with optional prefix and suffix tags or seperators.
		/// </summary>
		/// <returns>If the user is not logged in and not an admin, an empty string is returned.</returns>
		public static MvcHtmlString NewPageLink(this HtmlHelper helper, string prefix, string suffix)
		{
			if (RoadkillContext.Current.IsLoggedIn && (RoadkillContext.Current.IsAdmin || RoadkillContext.Current.IsEditor))
			{
				string link = helper.ActionLink(SiteStrings.Navigation_NewPage, "New", "Pages").ToString();
				return MvcHtmlString.Create(prefix + link + suffix);
			}
			else
			{
				return MvcHtmlString.Empty;
			}
		}

		/// <summary>
		/// Provides a link to the index page of the site, with optional prefix and suffix tags or seperators.
		/// </summary>
		public static MvcHtmlString MainPageLink(this HtmlHelper helper, string linkText, string prefix,string suffix)
		{
			return helper.ActionLink(linkText, "Index", "Home");
		}

		/// <summary>
		/// Provides a link to the page with the provided title, querying it in the database first.
		/// </summary>
		/// <returns>If the page is not found, the link text is returned.</returns>
		public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, string pageTitle)
		{
			return helper.PageLink(linkText, pageTitle, null,"","");
		}

		/// <summary>
		/// Provides a link to the page with the provided title, querying it in the database first,
		/// with optional prefix and suffix tags or seperators.
		/// </summary>
		/// <returns>If the page is not found, the link text is returned.</returns>
		public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, string pageTitle, string prefix, string suffix)
		{
			return helper.PageLink(linkText, pageTitle, null, prefix, suffix);
		}

		/// <summary>
		/// Provides a link to the page with the provided title, querying it in the database first,
		/// with optional prefix and suffix tags or seperators and html attributes.
		/// </summary>
		/// <param name="htmlAttributes">Any additional html attributes to add to the link</param>
		/// <returns>If the page is not found, the link text is returned.</returns>
		public static MvcHtmlString PageLink(this HtmlHelper helper, string linkText, string pageTitle, object htmlAttributes,string prefix,string suffix)
		{
			PageManager manager = ObjectFactory.GetInstance<PageManager>();
			PageSummary summary = manager.FindByTitle(pageTitle);
			if (summary != null)
			{
				string link = helper.ActionLink(linkText, "Index", "Wiki", new { id = summary.Id, title = pageTitle }, htmlAttributes).ToString();
				return MvcHtmlString.Create(prefix + link + suffix);
			}
			else
			{
				return MvcHtmlString.Create(linkText);
			}
		}

		/// <summary>
		/// Provides a CSS link tag for the CSS file provided. If the relative path does not begin with ~ then
		/// the Assets/Css folder is assumed.
		/// </summary>
		public static MvcHtmlString CssLink(this UrlHelper helper, string relativePath)
		{
			if (!relativePath.StartsWith("~"))
				relativePath = "~/Assets/CSS/" + relativePath;

			return MvcHtmlString.Create("<link href=\"" + helper.Content(relativePath) + "\" rel=\"stylesheet\" type=\"text/css\" />");
		}

		/// <summary>
		/// Provides a Javascript script tag for the Javascript file provided. If the relative path does not begin with ~ then
		/// the Assets/Scripts folder is assumed.
		/// </summary>
		public static MvcHtmlString ScriptLink(this UrlHelper helper, string relativePath)
		{
			if (!relativePath.StartsWith("~"))
				relativePath = "~/Assets/Scripts/" + relativePath;

			return MvcHtmlString.Create("<script type=\"text/javascript\" language=\"javascript\" src=\"" + helper.Content(relativePath) + "\"></script>");
		}

		/// <summary>
		/// Gets the localization strings for the jQuery validationEngine.
		/// </summary>
		public static MvcHtmlString ScriptLinkForValidationLocalization(this UrlHelper helper)
		{
			string path = "~/Assets/Scripts/languages/jquery.validationEngine-" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName + ".js";

			return MvcHtmlString.Create("<script type=\"text/javascript\" language=\"javascript\" src=\"" + helper.Content(path) + "\"></script>");
		}
	}
}
