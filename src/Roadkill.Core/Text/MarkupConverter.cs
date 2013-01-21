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
using Roadkill.Core.Files;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// A factory class for converting the system's markup syntax to HTML.
	/// </summary>
	public class MarkupConverter
	{
		private IConfigurationContainer _configuration;
		private IRepository _repository;
		private IMarkupParser _parser;
		private UrlHelper _urlHelper;
		private static Regex _imgFileRegex = new Regex("^File:", RegexOptions.IgnoreCase);

		public Func<string,string> AbsolutePathConverter { get; set; }
		public Func<int, string, string> InternalUrlForTitle { get; set; }
		public Func<string, string> NewPageUrlForTitle { get; set; }
		private List<string> _linkIgnorePrefixes;
		
		public IMarkupParser Parser
		{
			get { return _parser; }
		}

		/// <summary>
		/// Gets the current <see cref="IMarkupParser"/> for the <see cref="RoadkillSettings.MarkupType"/>
		/// </summary>
		/// <returns>An <see cref="IMarkupParser"/> for Creole,Markdown or Media wiki formats.</returns>
		public MarkupConverter(IConfigurationContainer configuration, IRepository repository)
		{
			AbsolutePathConverter = ConvertToAbsolutePath;
			InternalUrlForTitle = GetUrlForTitle;
			NewPageUrlForTitle = GetNewPageUrlForTitle;

			_linkIgnorePrefixes = new List<string>()
			{
				"http://",
				"https://",
				"www.",
				"mailto:",
				"#",
				"tag:"
			};

			_repository = repository;
			_configuration = configuration;

			string markupType = "creole";

			if (_configuration.SitePreferences != null && !string.IsNullOrEmpty(_configuration.SitePreferences.MarkupType))
				markupType = _configuration.SitePreferences.MarkupType.ToLower();

			switch (markupType)
			{
				case "markdown":
					_parser = new MarkdownParser();
					break;

				case "mediawiki":
					_parser = new MediaWikiParser(_configuration);
					break;

				case "creole":
				default:
					_parser = new CreoleParser(_configuration);
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
			CustomTokenParser tokenParser = new CustomTokenParser(_configuration);
			string html = tokenParser.ReplaceTokens(text);
			html = _parser.Transform(html);
			html = RemoveHarmfulTags(html);

			if (html.IndexOf("{TOC}") > -1)
			{
				TocParser parser = new TocParser();
				html = parser.InsertToc(html);
			}
			
			return html;
		}

		/// <summary>
		/// Adds the attachments folder as a prefix to all image URLs before the HTML <img> tag is written.
		/// </summary>
		private void ImageParsed(object sender, ImageEventArgs e)
		{
			if (!e.OriginalSrc.StartsWith("http://") && !e.OriginalSrc.StartsWith("www."))
			{
				string src = e.OriginalSrc;
				src = _imgFileRegex.Replace(src, "");

				string attachmentsPath = AttachmentFileHandler.GetAttachmentsPath(_configuration);
				string urlPath = attachmentsPath + (src.StartsWith("/") ? "" : "/") + src;
				e.Src = ConvertToAbsolutePath(urlPath);
			}
		}

		/// <summary>
		/// Handles internal links, and the 'attachment:' prefix for attachment links.
		/// </summary>
		private void LinkParsed(object sender, LinkEventArgs e)
		{
			if (!_linkIgnorePrefixes.Any(x =>  e.OriginalHref.StartsWith(x)))
			{
				string href = e.OriginalHref;
				string lowerHref = href.ToLower();
				string cssClass = "";

				if (lowerHref.StartsWith("attachment:") || lowerHref.StartsWith("~/"))
				{
					// Parse "attachments:" to add the attachments path to the front of the href
					if (lowerHref.StartsWith("attachment:"))
					{
						href = href.Remove(0,11);
						if (!href.StartsWith("/"))
							href = "/" + href;
					}
					else if (lowerHref.StartsWith("~/"))
					{
						href = href.Remove(0, 1);
					}

					string attachmentsPath = AttachmentFileHandler.GetAttachmentsPath(_configuration);
					href = ConvertToAbsolutePath(attachmentsPath) + href;
				}
				else
				{
					// Parse internal links

					// For markdown, only urls with "-" in them are valid, spaces are ignored.
					// So remove these, so a match is made. No url has a "-" in, so replacing them is ok.
					string title = href;
					if (Parser is MarkdownParser)
					{
						title = title.Replace("-", " ");
					}

					Page page = _repository.FindPageByTitle(title);
					if (page != null)
					{
						href = InternalUrlForTitle(page.Id, page.Title);
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
		}

		/// <summary>
		/// Strips script, link, style etc tags and javascript attributes. Based on http://htmlagilitypack.codeplex.com/discussions/24346
		/// </summary>
		private string RemoveHarmfulTags(string html)
		{
			HtmlDocument document = new HtmlDocument();
			document.LoadHtml(html);
			
			HtmlNodeCollection collection = document.DocumentNode.SelectNodes("//script|//link|//iframe|//frameset|//frame|//applet");
			if (collection != null)
			{
				foreach (HtmlNode node in collection)
				{
					node.ParentNode.RemoveChild(node, false);
				}
			}

			// Remove hrefs to java/j/vbscript URLs
			collection = document.DocumentNode.SelectNodes("//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
			if (collection != null)
			{

				foreach (HtmlNode node in collection)
				{
					node.SetAttributeValue("href", "#");
				}
			}

			// Remove img with refs to java/j/vbscript URLs
			collection = document.DocumentNode.SelectNodes("//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
			if (collection != null)
			{
				foreach (HtmlNode node in collection)
				{
					node.SetAttributeValue("src", "#");
				}
			}

			collection = document.DocumentNode.SelectNodes("//*[@onclick or @onmouseover or @onfocus or @onblur or @onmouseout or @ondoubleclick or @onload or @onunload]");
			if (collection != null)
			{
				foreach (HtmlNode node in collection)
				{
					node.Attributes.Remove("onFocus");
					node.Attributes.Remove("onBlur");
					node.Attributes.Remove("onClick");
					node.Attributes.Remove("onMouseOver");
					node.Attributes.Remove("onMouseOut");
					node.Attributes.Remove("onDoubleClick");
					node.Attributes.Remove("onLoad");
					node.Attributes.Remove("onUnload");
				}
			}

			return document.DocumentNode.InnerHtml;
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
			UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
			return helper.Content(relativeUrl);
		}

		private string GetUrlForTitle(int id, string title)
		{
			UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
			return helper.Action("Index", "Wiki", new { id = id, title = title.EncodeTitle() });
		}

		private string GetNewPageUrlForTitle(string title)
		{
			UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
			return helper.Action("New", "Pages", new { title = title });
		}
	}
}
