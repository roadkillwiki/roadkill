using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	public static class HtmlExtensions
	{
		public static string MarkdownToHtml(this string text)
		{
			// This can eventually come from a factory class that creates
			// the converter based on a web.config setting
			MarkdownConverter converter = new MarkdownConverter();
			return converter.ToHtml(text);
		}

		public static MvcHtmlString MarkdownToHtml(this HtmlHelper helper,string text)
		{
			// As with above
			MarkdownConverter converter = new MarkdownConverter();
			return MvcHtmlString.Create(converter.ToHtml(text));
		}

		public static MvcHtmlString LoginStatus(this HtmlHelper helper)
		{
			if (RoadkillContext.Current.IsLoggedIn)
				return MvcHtmlString.Create("Logged in as "+RoadkillContext.Current.CurrentUser);
			else
				return MvcHtmlString.Create("Not logged in");
		}

		public static MvcHtmlString PageTitle(this HtmlHelper helper)
		{
			return MvcHtmlString.Create(helper.ViewData["PageTitle"].ToString());
		}

		public static MvcHtmlString CssForTheme(this UrlHelper helper)
		{
			return MvcHtmlString.Create(helper.Content(RoadkillSettings.ThemePath + "/Theme.css"));
		}

		public static MvcHtmlString LoginLink(this HtmlHelper helper)
		{
			if (RoadkillContext.Current.IsLoggedIn)
				return helper.ActionLink("Logout", "Logout", "Home");
			else
				return helper.ActionLink("Login", "Login", "Home");
		}

		public static string ClassNameForTagSummary(this HtmlHelper helper, TagSummary tag)
		{
			string className = "";

			if (tag.Count > 10)
			{
				className = "tagcloud5";
			}
			else if (tag.Count >= 5 && tag.Count < 10)
			{
				className = "tagcloud4";
			}
			else if (tag.Count >= 3 && tag.Count < 5)
			{
				className = "tagcloud3";
			}
			else if (tag.Count > 1 && tag.Count < 3)
			{
				className = "tagcloud2";
			}
			else if (tag.Count == 1)
			{
				className = "tagcloud1";
			}

			return className;
		}
	}
}
