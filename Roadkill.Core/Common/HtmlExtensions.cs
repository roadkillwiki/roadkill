using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Roadkill.Core
{
	public static class HtmlExtensions
	{
		public static MvcHtmlString MarkdownToHtml(this HtmlHelper helper,string text)
		{
			Markdown markdown = new Markdown();
			return MvcHtmlString.Create(markdown.Transform(text));
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
