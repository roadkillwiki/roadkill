using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Plugins.Tokens
{
	public class SyntaxHighlighter
	{
		private string _replacePattern;

		public SyntaxHighlighter()
		{
			_replacePattern = "{{{<pre class=\"${lang}\">${code}</pre>}}}";
		}

		public string ReplaceContent(TextToken token, string html)
		{
			if (token.CachedRegex.IsMatch(html))
			{
				MatchCollection matches = token.CachedRegex.Matches(html);
				foreach (Match match in matches)
				{
					string language = match.Groups["lang"].Value;
					string code = match.Groups["code"].Value;

					html = Regex.Replace(html, token.SearchRegex, _replacePattern, token.CachedRegex.Options);
				}
			}

			return html;
		}
	}
}
