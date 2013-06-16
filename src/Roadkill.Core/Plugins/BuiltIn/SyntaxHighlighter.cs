using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Roadkill.Core.Plugins.BuiltIn
{
	public class SyntaxHighlighter : CustomVariablePlugin
	{
		internal static readonly string _regexString = @"\[\[\[code lang=(?'lang'.*?)\|(?'code'.*?)\]\]\]";
		internal static readonly Regex _variableRegex = new Regex(_regexString, RegexOptions.Singleline | RegexOptions.Compiled);
		internal static readonly string _replacePattern = "<pre class=\"brush: ${lang}\">${code}</pre>";

		public override string Name
		{
			get
			{
				return "Syntax highlighting";
			}
		}

		public override string Description
		{
			get
			{
				return "Syntax highlights the content, using the language you specify. Example:<br/>" +
						"[[[code lang=sql|ENTER YOUR CONTENT HERE]]]";
			}
		}

		static SyntaxHighlighter()
		{
			_replacePattern = CustomVariablePlugin.AddParserIgnoreTokens(_replacePattern);
		}

		public override string BeforeParse(string text)
		{
			if (_variableRegex.IsMatch(text))
			{
				MatchCollection matches = _variableRegex.Matches(text);
				foreach (Match match in matches)
				{
					string language = match.Groups["lang"].Value;
					string code = HttpUtility.HtmlEncode(match.Groups["code"].Value);

					text = Regex.Replace(text, _regexString, _replacePattern, _variableRegex.Options);
				}
			}

			return text;
		}

		public override string GetHeadContent(UrlHelper urlHelper)
		{
			string cssLink = "<link href=\"~/Assets/CSS/plugins/syntaxhighlighter/{0}\" rel=\"stylesheet\" type=\"text/css\" />\n";
			string jsScript = "<script src=\"~/Assets/Scripts/plugins/syntaxhighlighter/{0}\" type=\"text/javascript\"></script>\n";
			string html = "";

			foreach (string file in HeadContent.CssFiles)
			{
				html += string.Format(cssLink, urlHelper.Content(file));
			}

			foreach (string file in HeadContent.JsFiles)
			{
				html += string.Format(jsScript, urlHelper.Content(file));
			}

			html += "<script type=\"text/javascript\">SyntaxHighlighter.all()</script>";

			return html;
		}

		private class HeadContent
		{
			public static string[] CssFiles = 
			{
				"shCore.css",
				"shThemeDefault.css"
			};

			public static string[] JsFiles = 
			{
				"shCore.js",
				"shBrushJScript.js"
			};
		}
	}
}
