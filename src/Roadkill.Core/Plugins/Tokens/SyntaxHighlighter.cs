using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Plugins.Tokens
{
	public class SyntaxHighlighter
	{
		internal static string _noParseStartToken = "\t\t\t\t{{{roadkillinternal";
		internal static string _noParseEndToken = "roadkillinternal}}}";
		internal static string _replacePattern = _noParseStartToken + "\n<pre class=\"${lang}\">${code}</pre>\n" + _noParseEndToken;

		public string BeforeParse(TextToken token, string text)
		{
			if (token.CachedRegex.IsMatch(text))
			{
				MatchCollection matches = token.CachedRegex.Matches(text);
				foreach (Match match in matches)
				{
					string language = match.Groups["lang"].Value;
					string code = match.Groups["code"].Value;

					text = Regex.Replace(text, token.SearchRegex, _replacePattern, token.CachedRegex.Options);
				}
			}

			return text;
		}

		public string AfterParse(TextToken token, string html)
		{
			html = html.Replace(_noParseStartToken, "");
			html = html.Replace(_noParseEndToken, "");

			return html;
		}
	}
}
