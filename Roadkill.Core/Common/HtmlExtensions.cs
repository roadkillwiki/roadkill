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
		public static MvcHtmlString MarkdownToHtml(this HtmlHelper helper, string content)
		{
			return MvcHtmlString.Create(content.WikiMarkupToHtml());
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
			if (helper.ViewData == null)
				return MvcHtmlString.Create("");
			else
				return MvcHtmlString.Create(helper.ViewData["PageTitle"].ToString());
		}

		public static MvcHtmlString LoginLink(this HtmlHelper helper)
		{
			string link = "";

			if (RoadkillContext.Current.IsLoggedIn)
				link = helper.ActionLink("Logout", "Logout", "Home").ToString();
			else
				link = helper.ActionLink("Login", "Login", "Home").ToString();

			return MvcHtmlString.Create(link +  "&nbsp;|&nbsp;");
		}

		public static MvcHtmlString NewPageLink(this HtmlHelper helper)
		{
			if (RoadkillContext.Current.IsLoggedIn)
				return MvcHtmlString.Create(helper.ActionLink("New page", "New", "Page").ToString() + "&nbsp;|&nbsp;");
			else
				return MvcHtmlString.Empty;
		}

		/// <summary>
		/// Simplifies (at the expense of being ugly to maintain) the required css/javascript file includes for themes.
		/// </summary>
		/// <param name="helper"></param>
		/// <returns></returns>
		public static MvcHtmlString HeadContent(this UrlHelper helper)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine("<!-- ## REQUIRED ## -->");

			builder.AppendLine("<script type=\"text/javascript\" language=\"javascript\" src=\"" + 
				helper.Action("JavascriptSettingsForEditing", "Home") + "\"></script>");

			builder.AppendLine("<link rel=\"shortcut icon\" href=\"" +helper.Content("~/Assets/Images/favicon.png")+ "\" />");
			builder.AppendLine(CssLink(helper,"~/Assets/Css/roadkill.css"));
			builder.AppendLine(ScriptLink(helper,"~/Assets/Scripts/jquery-1.4.1.min.js"));
			builder.AppendLine(ScriptLink(helper,"~/Assets/Scripts/roadkill.js"));
			builder.AppendLine(CssLink(helper,RoadkillSettings.ThemePath + "/Theme.css"));
			builder.AppendLine("<!-- ## END REQUIRED ## -->");

			return MvcHtmlString.Create(builder.ToString());
		}

		public static string CssLink(this UrlHelper helper,string relativePath)
		{
			return "<link href=\"" +helper.Content(relativePath) +"\" rel=\"stylesheet\" type=\"text/css\" />";
		}

		public static string ScriptLink(this UrlHelper helper,string relativePath)
		{
			return "<script type=\"text/javascript\" language=\"javascript\" src=\"" +helper.Content(relativePath) +"\"></script>";
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
