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

		public override string Id
		{
			get 
			{ 
				return "SyntaxHighlighter";	
			}
		}

		public override string Name
		{
			get
			{
				return "Syntax Highlighter";
			}
		}

		public override string Description
		{
			get
			{
				return "Syntax highlights a code block, using the language you specify. Example:<br/>" +
						"[[[code lang=sql|ENTER YOUR CODE HERE]]]";
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
			string html = "";

			foreach (string file in HeadContent.CssFiles)
			{
				html += GetCssLink(urlHelper, file);
			}

			foreach (string file in HeadContent.JsFiles)
			{
				html += GetScriptLink(urlHelper, file);
			}

			html += "\t\t<script type=\"text/javascript\">SyntaxHighlighter.all()</script>\n";

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
				"shCore.js", // needs to be 1st
				"shBrushAppleScript.js",
				"shBrushAS3.js",
				"shBrushBash.js",
				"shBrushColdFusion.js",
				"shBrushCpp.js",
				"shBrushCSharp.js",
				"shBrushCss.js",
				"shBrushDelphi.js",
				"shBrushDiff.js",
				"shBrushErlang.js",
				"shBrushGroovy.js",
				"shBrushJava.js",
				"shBrushJavaFX.js",
				"shBrushJScript.js",
				"shBrushPerl.js",
				"shBrushPhp.js",
				"shBrushPlain.js",
				"shBrushPowerShell.js",
				"shBrushPython.js",
				"shBrushRuby.js",
				"shBrushSass.js",
				"shBrushScala.js",
				"shBrushSql.js",
				"shBrushVb.js",
				"shBrushXml.js",
			};
		}
	}
}
