using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Roadkill.Core
{
	public static class HtmlExtensions
	{
		public static MvcHtmlString MarkdownToHtml(this HtmlHelper helper,string text)
		{
			Markdown markdown = new Markdown();
			return MvcHtmlString.Create(markdown.Transform(text));
		}

		public static bool ToBool(this ViewDataDictionary dictionary,string key)
		{
			if (!dictionary.ContainsKey(key))
				return false;

			bool result;
			if (bool.TryParse(dictionary[key].ToString(), out result))
				return result;
			else
				return false;
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
