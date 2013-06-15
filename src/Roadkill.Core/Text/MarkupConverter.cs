using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Roadkill.Core.Configuration;
using StructureMap;
using System.IO;
using Roadkill.Core.Attachments;
using Roadkill.Core.Text.Sanitizer;
using Roadkill.Core.Database;
using Roadkill.Core.Text;
using Roadkill.Core.Text.ToC;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// A factory class for converting the system's markup syntax to HTML.
	/// </summary>
	public class MarkupConverter
	{
		private ApplicationSettings _applicationSettings;
		private IRepository _repository;
		private IMarkupParser _parser;
		private UrlHelper _urlHelper;
		private static Regex _imgFileRegex = new Regex("^File:", RegexOptions.IgnoreCase);
		private static Regex _anchorRegex = new Regex("(?<hash>[#|%23].+)", RegexOptions.IgnoreCase);

		/// <summary>
		/// A method used by the converter to convert absolute paths to relative paths.
		/// </summary>
		public Func<string,string> AbsolutePathConverter { get; set; }

		/// <summary>
		/// A method used by the converter to get the internal url of a page based on the page title.
		/// </summary>
		public Func<int, string, string> InternalUrlForTitle { get; set; }

		/// <summary>
		/// A method used by the converter to get the url for adding a new page.
		/// </summary>
		public Func<string, string> NewPageUrlForTitle { get; set; }
		private List<string> _externalLinkPrefixes;
		
		/// <summary>
		/// The current <see cref="IMarkupParser"/> being used by this instance, which is taken from 
		/// the configuration markdown type setting.
		/// </summary>
		public IMarkupParser Parser
		{
			get { return _parser; }
		}

		//[SetterProperty]
		/// <summary>
		/// Creates a new markdown parser which handles the image and link parsing by the various different 
		/// markdown format parsers.
		/// </summary>
		/// <returns>An <see cref="IMarkupParser"/> for Creole,Markdown or Media wiki formats.</returns>
		public MarkupConverter(ApplicationSettings settings, IRepository repository)
		{
			AbsolutePathConverter = ConvertToAbsolutePath;
			InternalUrlForTitle = GetUrlForTitle;
			NewPageUrlForTitle = GetNewPageUrlForTitle;

			_externalLinkPrefixes = new List<string>()
			{
				"http://",
				"https://",
				"www.",
				"mailto:",
				"#",
				"tag:"
			};

			_repository = repository;
			_applicationSettings = settings;

			string markupType = "";
	
			if (!_applicationSettings.Installed || _applicationSettings.UpgradeRequired)
			{
				string warnMessage = "Roadkill is not installed, or an upgrade is pending (ApplicationSettings.UpgradeRequired = false)." +
									"Skipping initialization of MarkupConverter (MarkupConverter.Parser will now be null)";

				Console.WriteLine(warnMessage);
				Log.Warn(warnMessage);

				// Skip the chain of creation, as the markup converter isn't needed
				return;
			}

			SiteSettings siteSettings = repository.GetSiteSettings();
			if (siteSettings != null && !string.IsNullOrEmpty(siteSettings.MarkupType))
			{
				markupType = siteSettings.MarkupType.ToLower();
			}

			switch (markupType)
			{
				case "markdown":
					_parser = new MarkdownParser();
					break;

				case "mediawiki":
					_parser = new MediaWikiParser(_applicationSettings, siteSettings);
					break;

				case "creole":
				default:
					_parser = new CreoleParser(_applicationSettings, siteSettings);
					break;
			}

			_parser.LinkParsed += LinkParsed;
			_parser.ImageParsed += ImageParsed;
		}

		/// <summary>
		/// Turns the wiki markup provided into HTML.
		/// </summary>
		/// <param name="text">A wiki markup string, e.g. creole markup.</param>
		/// <returns>The wiki markup converted to HTML.</returns>
		public string ToHtml(string text)
		{
			CustomTokenParser tokenParser = new CustomTokenParser(_applicationSettings);
			text = tokenParser.ReplaceTokensBeforeParse(text);

			string html = _parser.Transform(text);
			html = RemoveHarmfulTags(html);

			html = tokenParser.ReplaceTokensAfterParse(html);

			TocParser parser = new TocParser();
			html = parser.InsertToc(html);

			return html;
		}

		/// <summary>
		/// Adds the attachments folder as a prefix to all image URLs before the HTML &lt;img&gt; tag is written.
		/// </summary>
		private void ImageParsed(object sender, ImageEventArgs e)
		{
			if (!e.OriginalSrc.StartsWith("http://") && !e.OriginalSrc.StartsWith("https://") && !e.OriginalSrc.StartsWith("www."))
			{
				string src = e.OriginalSrc;
				src = _imgFileRegex.Replace(src, "");

				string attachmentsPath = _applicationSettings.AttachmentsUrlPath;
				string urlPath = attachmentsPath + (src.StartsWith("/") ? "" : "/") + src;
				e.Src = AbsolutePathConverter(urlPath);
			}
		}

		/// <summary>
		/// Handles internal links, and the 'attachment:' prefix for attachment links.
		/// </summary>
		private void LinkParsed(object sender, LinkEventArgs e)
		{
			if (!_externalLinkPrefixes.Any(x => e.OriginalHref.StartsWith(x)))
			{
				string href = e.OriginalHref;
				string lowerHref = href.ToLower();
				string cssClass = "";

				if (lowerHref.StartsWith("attachment:") || lowerHref.StartsWith("~/"))
				{
					// Parse "attachments:" to add the attachments path to the front of the href
					if (lowerHref.StartsWith("attachment:"))
					{
						href = href.Remove(0, 11);
						if (!href.StartsWith("/"))
							href = "/" + href;
					}
					else if (lowerHref.StartsWith("~/"))
					{
						href = href.Remove(0, 1);
					}

					string attachmentsPath = _applicationSettings.AttachmentsUrlPath;
					href = AbsolutePathConverter(attachmentsPath) + href;
				}
				else
				{
					// Parse internal links
					string title = href;
					string anchorHash = "";

					// Parse anchors for other pages
					if (_anchorRegex.IsMatch(href))
					{
						// Grab the hash contents
						Match match = _anchorRegex.Match(href);
						anchorHash = match.Groups["hash"].Value;

						// Grab the url
						title = href.Replace(anchorHash, "");
					}

					if (Parser is MarkdownParser)
					{
						// For markdown, only urls with "-" in them are valid, spaces are ignored.
						// So remove these, so a match is made. No url has a "-" in, so replacing them is ok.
						title = title.Replace("-", " ");
					}
					

					Page page = _repository.GetPageByTitle(title);
					if (page != null)
					{
						href = InternalUrlForTitle(page.Id, page.Title);
						href += anchorHash;
					}
					else
					{
						href = NewPageUrlForTitle(href);
						cssClass = "missing-page-link";
					}
				}

				e.Href = href;
				e.Target = "";
				e.CssClass = cssClass;
			}
			else
			{
				e.CssClass = "external-link";
			}
		}

		/// <summary>
		/// Strips a lot of unsafe Javascript/Html/CSS from the markup, if the feature is enabled.
		/// </summary>
		private string RemoveHarmfulTags(string html)
		{
			if (_applicationSettings.UseHtmlWhiteList)
			{
				MarkupSanitizer sanitizer = new MarkupSanitizer(_applicationSettings, true, false, true);
				return sanitizer.SanitizeHtml(html);
			}
			else
			{
				return html;
			}
		}

		/// <summary>
		/// Whether the text provided contains any links to the page title.
		/// </summary>
		/// <param name="text">The page's text contents.</param>
		/// <param name="pageName">The name (title) of the page.</param>
		/// <returns>True if the text contains links; false otherwise.</returns>
		public bool ContainsPageLink(string text, string pageName)
		{
			Regex regex = new Regex(GetLinkUpdateRegex(pageName), RegexOptions.IgnoreCase);
			return regex.IsMatch(text);
		}

		/// <summary>
		/// Replaces all links with an old page title in the provided page text, with links with a new page name.
		/// </summary>
		/// <param name="text">The page's text contents.</param>
		/// <param name="pageName">The previous name (title) of the page.</param>
		/// <param name="newPageName">The new name (title) of the page.</param>
		/// <returns>The text with link title names replaced.</returns>
		public string ReplacePageLinks(string text, string oldPageName, string newPageName)
		{
			Regex regex = new Regex(GetLinkUpdateRegex(oldPageName), RegexOptions.IgnoreCase);
			return regex.Replace(text, delegate(Match match)
			{
				if (match.Success && match.Groups.Count == 2)
				{
					return match.Value.Replace(match.Groups[1].Value, newPageName);
				}
				else
				{
					return match.Value;
				}
			});
		}

		/// <summary>
		/// Gets a regex to update all links in a page.
		/// </summary>
		private string GetLinkUpdateRegex(string pageName)
		{
			string regex = string.Format("{0}{1}", _parser.LinkStartToken, _parser.LinkEndToken);
			regex = regex.Replace("%LINKTEXT%", "(?:.*?)");
			regex = regex.Replace("(", @"\(").Replace(")", @"\)").Replace("[", @"\[").Replace("]", @"\]");
			regex = regex.Replace("%URL%", "(?<url>" + pageName + ")"); // brackets or square brackets will break the URL, so ignore these.

			return regex;
		}

		private string ConvertToAbsolutePath(string relativeUrl)
		{
			if (HttpContext.Current != null)
			{
				UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				return helper.Content(relativeUrl);
			}
			else
			{
				return relativeUrl;
			}
		}

		private string GetUrlForTitle(int id, string title)
		{
			if (HttpContext.Current != null)
			{
				UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				return helper.Action("Index", "Wiki", new { id = id, title = title.EncodeTitle() });
			}
			else
			{
				return string.Format("/index/wiki/{0}/{1}", id, title);
			}
		}

		private string GetNewPageUrlForTitle(string title)
		{
			if (HttpContext.Current != null)
			{
				UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				return helper.Action("New", "Pages", new { title = title });
			}
			else
			{
				return string.Format("/pages/new/?title={0}", title);
			}
		}
	}
}
