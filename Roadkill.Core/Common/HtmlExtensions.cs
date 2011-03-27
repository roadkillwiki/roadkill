using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Roadkill.Core.Converters;
using System.Web;
using System.Text.RegularExpressions;

namespace Roadkill.Core
{
	public static class HtmlExtensions
	{
		public static MvcHtmlString TagBlocks(this HtmlHelper helper, string content)
		{
			string result = "";

			if (!string.IsNullOrWhiteSpace(content))
			{
				string[] parts = content.Split(';');

				StringBuilder builder = new StringBuilder();
				foreach (string item in parts)
				{
					if (!string.IsNullOrWhiteSpace(item))
					{
						string url = helper.ActionLink(item, "Tag", "Pages", new { id = item },null).ToString();
						builder.AppendFormat("<span class=\"tagblock\">{0}</span>", url);
					}
				}

				result = builder.ToString();
			}

			return MvcHtmlString.Create(result);
		}

		public static string EncodeTitle(this string title)
		{
			return EncodeTitleInternal(title);
		}

		public static MvcHtmlString EncodeTitle(this UrlHelper helper, string title)
		{
			title = EncodeTitleInternal(title);
			return MvcHtmlString.Create(title);
		}

		private static string EncodeTitleInternal(string title)
		{
			if (string.IsNullOrEmpty(title))
				return title;

			// Search engine friendly slug routine with help from http://www.intrepidstudios.com/blog/2009/2/10/function-to-generate-a-url-friendly-string.aspx
			
			// remove invalid characters
			title = Regex.Replace(title, @"[^\w\d\s-]", "");  // this is unicode safe, but may need to revert back to 'a-zA-Z0-9', need to check spec
			
			// convert multiple spaces/hyphens into one space       
			title = Regex.Replace(title, @"[\s-]+", " ").Trim(); 
			
			// If it's over 30 chars, take the first 30.
			title = title.Substring(0, title.Length <= 75 ? title.Length : 75).Trim(); 
			
			// hyphenate spaces
			title = Regex.Replace(title, @"\s", "-");

			return title;
		}

		public static MvcHtmlString WikiMarkupToHtml(this HtmlHelper helper, string content)
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

		public static string ThemeContent(this UrlHelper helper, string relativePath)
		{
			return helper.Content(RoadkillSettings.ThemePath + "/" + relativePath);
		}

		public static string FormatFileSize(this HtmlHelper helper,int size)
		{
			if (size > 1024)
				return size / 1024 + "KB";
			else
				return size + " bytes";
		}
	}
}
