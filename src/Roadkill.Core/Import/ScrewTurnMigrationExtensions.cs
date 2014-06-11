using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Import
{
	/// <summary>
	/// Extension methods for migrating ScrewTurn content
	/// </summary>
	public static class ScrewTurnMigrationExtensions
	{
		/// <summary>
		/// Replaces hyperlinks formatted using ScrewTurn markup
		/// </summary>
		public static string ReplaceHyperlinks(this string text, Dictionary<string, string> nameTitleMapping)
		{
			Regex re = new Regex(@"(?<LinkMarkup>\[\^?(?<LinkUri>.*?)(?:\|(?<LinkText>.*?))?])", RegexOptions.Singleline);
			MatchCollection matches = re.Matches(text);
			IEnumerable<Match> uniqueMatches = matches.OfType<Match>().Distinct(new MatchComparer());
			foreach (Match match in uniqueMatches)
			{
				string linkMarkup = match.Groups["LinkMarkup"].Value;
				string linkUri = match.Groups["LinkUri"].Value;
				string linkText = match.Groups["LinkText"].Value;

				if (nameTitleMapping.ContainsKey(linkUri))
				{
					linkUri = nameTitleMapping[linkUri];
				}

				if (linkUri == linkText)
				{
					linkText = "";
				}
				else if (!string.IsNullOrEmpty(linkText))
				{
					linkText = "|" + linkText;
				}
			  
				string newLinkMarkup = string.Format("[[{0}{1}]]", linkUri, linkText);
				newLinkMarkup = newLinkMarkup.Replace("{UP}", "/");
				text = text.Replace(linkMarkup, newLinkMarkup);
			}
			return text;
		}

		/// <summary>
		/// Replaces line breaks formatted using ScrewTurn markup
		/// </summary>
		public static string ReplaceBr(this string text)
		{
			return Regex.Replace(text, "{BR}", "\n", RegexOptions.IgnoreCase);
		}

		/// <summary>
		/// Replaces image links formatted using ScrewTurn markup. Should be executed after replacing hyperlinks.
		/// </summary>
		public static string ReplaceImageLinks(this string text)
		{
			Regex re = new Regex(@"(?<ImageMarkup>\[\[image.*?\|(?<Title>.*?)\|(?:/|{UP(?<PageAttachment>.*?)})(?<Path>.+?)]])", RegexOptions.Singleline);
			MatchCollection matches = re.Matches(text);
			IEnumerable<Match> uniqueMatches = matches.OfType<Match>().Distinct(new MatchComparer());
			foreach (Match match in uniqueMatches)
			{
				string imageMarkup = match.Groups["ImageMarkup"].Value;
				string title = match.Groups["Title"].Value;
				string pageAttachment = match.Groups["PageAttachment"].Value;
				string path = match.Groups["Path"].Value;

				string newPathPrefix = "/";
				if (!string.IsNullOrEmpty(pageAttachment))
				{
					newPathPrefix = "/Images/";
				}
				if (!string.IsNullOrEmpty(title))
				{
					title = "|" + title;
				}

				const string MediaWikiFormat = "[[File:{0}{1}{2}]]";
				const string CreoleWikiFormat = "{{{0}{1}{2}}}";
				string newImageMarkup = string.Format(MediaWikiFormat, newPathPrefix, path, title);

				text = text.Replace(imageMarkup, newImageMarkup);
			}

			return text;
		}

		/// <summary>
		/// Replaces block code formatted using ScrewTurn markup. Should be executed before replacing inline code.
		/// </summary>
		public static string ReplaceBlockCode(this string text)
		{
			Regex re = new Regex(@"@@(?<Code>.+?)@@|{{{{(?<Code>.+?)}}}}", RegexOptions.Singleline);
			return re.Replace(text, "[[[code lang=|${Code}]]]");
		}

		/// <summary>
		/// Replaces inline code formatted using ScrewTurn markup. Should be executed after replacing block code.
		/// </summary>
		public static string ReplaceInlineCode(this string text)
		{
			Regex re = new Regex(@"{{(?<Code>.+?)}}", RegexOptions.Singleline);
			return re.Replace(text, "<code>${Code}</code>");
		}

		/// <summary>
		/// Replaces box markup formatted using ScrewTurn markup
		/// </summary>
		public static string ReplaceBoxMarkup(this string text)
		{
			Regex re = new Regex(@"\(\(\((?<Content>.+?)\)\)\)", RegexOptions.Singleline);
			return re.Replace(text, "@@infobox:${Content}@@");
		}
	}
}