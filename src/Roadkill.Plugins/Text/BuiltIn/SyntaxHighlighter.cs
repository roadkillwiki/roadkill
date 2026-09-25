using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Roadkill.Core.Plugins;

namespace Roadkill.Plugins.Text.BuiltIn
{
	public class SyntaxHighlighter : TextPlugin
	{
		internal static readonly string RegexString = @"\[\[\[code lang=(?'lang'.*?)\|(?'code'.*?)\]\]\]";
		internal static readonly Regex CompiledRegex = new Regex(RegexString, RegexOptions.Singleline | RegexOptions.Compiled);
		internal static string ReplacementPattern = "<pre class=\"brush: ${lang}\">${code}</pre>";

		// Markdown fenced code blocks (```sql) are output as <pre><code class="language-sql">
		internal static readonly Regex FencedCodeRegex = new Regex(@"<pre><code class=""language-(?'lang'[^""]+)"">(?'code'.*?)</code></pre>", RegexOptions.Singleline | RegexOptions.Compiled);

		// The languages (aliases) supported by the SyntaxHighlighter brushes that are loaded.
		internal static readonly HashSet<string> SupportedLanguages = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"applescript", "actionscript3", "as3", "bash", "shell", "c#", "c-sharp", "csharp", "coldfusion", "cf", "cpp", "c",
			"css", "delphi", "pascal", "pas", "diff", "patch", "erl", "erlang", "groovy", "java", "jfx", "javafx",
			"js", "jscript", "javascript", "perl", "pl", "php", "text", "plain", "powershell", "ps", "py", "python",
			"ruby", "rails", "ror", "rb", "sass", "scss", "scala", "sql", "vb", "vbnet", "xml", "xhtml", "xslt", "html"
		};

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
				return "Syntax highlights a code block, using the language you specify. Example:\n\n" +
						"[[[code lang=sql|ENTER YOUR CODE HERE]]]\n\n" +
						"Markdown fenced code blocks (```sql) are also highlighted.";
			}
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		static SyntaxHighlighter()
		{
			ReplacementPattern = ParserSafeToken(ReplacementPattern);
		}

		public override string BeforeParse(string markupText)
		{
			if (CompiledRegex.IsMatch(markupText))
			{
				// Replaces the [[[code lang=sql|xxx]]]
				// with the HTML tags (surrounded with {{{roadkillinternal}}.
				// As the code is HTML encoded, it doesn't get butchered by the HTML cleaner.
				MatchCollection matches = CompiledRegex.Matches(markupText);
				foreach (Match match in matches)
				{
					string language = match.Groups["lang"].Value;
					string code = HttpUtility.HtmlEncode(match.Groups["code"].Value);
					markupText = markupText.Replace(match.Groups["code"].Value, code);

					markupText = Regex.Replace(markupText, RegexString, ReplacementPattern, CompiledRegex.Options);
				}
			}

			return markupText;
		}

		public override string AfterParse(string html)
		{
			html = RemoveParserIgnoreTokens(html);

			// Undo the HTML sanitizer's attribute cleaning on the pre's.
			html = html.Replace("<pre class=\"brush&#x3A;&#x20;c&#x23;", "<pre class=\"brush: c#");
			html = html.Replace("<pre class=\"brush&#x3A;&#x20;", "<pre class=\"brush: ");

			// Highlight Markdown fenced code blocks for the languages the brushes support.
			html = FencedCodeRegex.Replace(html, match =>
			{
				string language = WebUtility.HtmlDecode(match.Groups["lang"].Value);
				if (!SupportedLanguages.Contains(language))
					return match.Value;

				return string.Format("<pre class=\"brush: {0}\">{1}</pre>", language.ToLowerInvariant(), match.Groups["code"].Value);
			});

			return html;
		}

		public override string GetHeadContent()
		{
			string html = "";

			foreach (string file in HeadContent.CssFiles)
			{
				html += GetCssLink("css/" +file);
			}

			foreach (string file in HeadContent.JsFiles)
			{
				AddScript("javascript/" + file);
			}

			SetHeadJsOnLoadedFunction("SyntaxHighlighter.all()");
			html += GetJavascriptHtml();

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
