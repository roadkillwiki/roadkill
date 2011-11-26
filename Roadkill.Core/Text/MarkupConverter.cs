using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Converters
{
	/// <summary>
	/// A factory class for converting the system's markup syntax to HTML.
	/// </summary>
	public class MarkupConverter
	{
		[ThreadStatic]
		private static IParser _parser;
		private static Regex _imgFileRegex = new Regex("^File:", RegexOptions.IgnoreCase);

		/// <summary>
		/// Gets the current <see cref="IParser"/> for the <see cref="RoadkillSettings.MarkupType"/>
		/// </summary>
		/// <returns>An <see cref="IParser"/> for Creole,Markdown or Media wiki formats.</returns>
		public IParser Parser
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

				e.Src = helper.Content(RoadkillSettings.AttachmentsFolder + "/" + src);
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

				if (href.ToLower().StartsWith("attachment:"))
				{
					// Parse "attachments:" to add the attachments path to the front of the href
					href = href.Remove(0,11);
					if (!href.StartsWith("/"))
						href = "/" + href;

					href = helper.Content(RoadkillSettings.AttachmentsFolder) + href;
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
	}
}
