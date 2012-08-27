using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// A factory class for converting the system's markup syntax to HTML.
	/// </summary>
	public class MarkupConverter
	{
		[ThreadStatic]
		private static IMarkupParser _parser;
		private static Regex _imgFileRegex = new Regex("^File:", RegexOptions.IgnoreCase);

		/// <summary>
		/// Gets the current <see cref="IMarkupParser"/> for the <see cref="RoadkillSettings.MarkupType"/>
		/// </summary>
		/// <returns>An <see cref="IMarkupParser"/> for Creole,Markdown or Media wiki formats.</returns>
		public IMarkupParser Parser
		{
			get
			{
				if (_parser == null)
				{
					switch (RoadkillSettings.MarkupType.ToLower())
					{
						case "markdown":
							_parser = new MarkdownParser();
							break;

						case "mediawiki":
							_parser = new MediaWikiParser();
							break;

						case "creole":
						default:
							_parser = new CreoleParser();
							break;
					}

					_parser.LinkParsed += LinkParsed;
					_parser.ImageParsed += ImageParsed;
				}

				return _parser;
			}
		}

		/// <summary>
		/// Turns the wiki markup provided into HTML.
		/// </summary>
		/// <param name="text">A wiki markup string, e.g. creole markup.</param>
		/// <returns>The wiki markup converted to HTML.</returns>
		public string ToHtml(string text)
		{
			string html = CustomTokenParser.ReplaceTokens(text);
			html = Parser.Transform(html);
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
			UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);

			if (!e.OriginalSrc.StartsWith("http://") && !e.OriginalSrc.StartsWith("www."))
			{
				string src = e.OriginalSrc;
				src = _imgFileRegex.Replace(src, "");

				string urlPath = RoadkillSettings.AttachmentsUrlPath + (src.StartsWith("/") ? "" : "/") + src;
				e.Src = helper.Content(urlPath);
			}
		}

		/// <summary>
		/// Handles internal links, and the 'attachment:' prefix for attachment links.
		/// </summary>
		private void LinkParsed(object sender, LinkEventArgs e)
		{
			UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);

			if (!e.OriginalHref.StartsWith("http://") && !e.OriginalHref.StartsWith("www.") && !e.OriginalHref.StartsWith("mailto:"))
			{
				string href = e.OriginalHref;
				string lowerHref = href.ToLower();

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

					href = helper.Content(RoadkillSettings.AttachmentsUrlPath) + href;
				}
				else
				{
					// Parse internal links
					PageManager manager = new PageManager();
					PageSummary summary = manager.FindByTitle(href);
					if (summary != null)
						href = helper.Action("Index", "Wiki", new { id = summary.Id, title = summary.Title.EncodeTitle() });

				}
				e.Href = href;
				e.Target = "";
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

		private string GetLinkUpdateRegex(string pageName)
		{
			string regex = string.Format("{0}{1}", Parser.LinkStartToken, Parser.LinkEndToken);
			regex = regex.Replace("%LINKTEXT%", "(?:.*?)");
			regex = regex.Replace("(", @"\(").Replace(")", @"\)").Replace("[", @"\[").Replace("]", @"\]");
			regex = regex.Replace("%URL%", "(?<url>" + pageName + ")"); // brackets or square brackets will break the URL, so ignore these.

			return regex;
		}
	}
}
