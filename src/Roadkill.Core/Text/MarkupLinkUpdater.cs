using System.Text.RegularExpressions;

namespace Roadkill.Core.Converters
{
	public class MarkupLinkUpdater
	{
		private IMarkupParser _parser;

		public MarkupLinkUpdater(IMarkupParser parser)
		{
			_parser = parser;
		}

		/// <summary>
		/// Whether the text provided contains any links to the page title.
		/// </summary>
		/// <param name="text">The page's text contents.</param>
		/// <param name="pageName">The name (title) of the page.</param>
		/// <returns>True if the text contains links; false otherwise.</returns>
		public bool ContainsPageLink(string text, string pageName)
		{
			if (string.IsNullOrEmpty(text))
				return false;

			pageName = AddDashesForMarkdownTitle(pageName);
			string customRegex = GetRegexForTitle(pageName);

			Regex regex = new Regex(customRegex, RegexOptions.IgnoreCase);
			return regex.IsMatch(text);
		}

		/// <summary>
		/// Replaces all links with an old page title in the provided page text, with links with a new page name.
		/// </summary>
		/// <param name="text">The page's text contents.</param>
		/// <param name="oldPageName">The previous name (title) of the page.</param>
		/// <param name="newPageName">The new name (title) of the page.</param>
		/// <returns>The text with link title names replaced.</returns>
		public string ReplacePageLinks(string text, string oldPageName, string newPageName)
		{
			oldPageName = AddDashesForMarkdownTitle(oldPageName);
			newPageName = AddDashesForMarkdownTitle(newPageName, false);

			string customRegex = GetRegexForTitle(oldPageName);
			Regex regex = new Regex(customRegex, RegexOptions.IgnoreCase);

			return regex.Replace(text, (Match match) => 
			{
				return OnLinkMatched(match, newPageName);
			});
		}

		private string OnLinkMatched(Match match, string newPageName)
		{
			if (match.Success && match.Groups.Count == 3)
			{
				if (!string.IsNullOrEmpty(match.Groups["url"].Value))
					return match.Value.Replace(match.Groups["url"].Value, newPageName);
			}

			return match.Value;
		}

		/// <summary>
		/// Gets a regex to update all links in a page.
		/// </summary>
		private string GetRegexForTitle(string pageName)
		{
			string regex = _parser.LinkStartToken;
			regex += _parser.LinkEndToken;
			regex = EscapeRegex(regex);
			regex = regex.Replace("%LINKTEXT%", "(?<name>.+?)");
			regex = regex.Replace("%URL%", "(?<url>" + pageName + "+?)");

			return regex;
		}

		private string EscapeRegex(string regex)
		{
			regex = regex.Replace("|", @"\|")
				.Replace("(", @"\(")
				.Replace(")", @"\)")
				.Replace("[", @"\[")
				.Replace("]", @"\]");

			return regex;
		}

		private string AddDashesForMarkdownTitle(string title, bool escape = true)
		{
			if (_parser is MarkdownParser)
			{
				if (escape)
					title = title.Replace(" ", @"\-");
				else
					title = title.Replace(" ", "-");
			}

			return title;
		}
	}
}