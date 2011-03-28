using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters.Creole;
using System.Web.Mvc;
using System.Web;
using Roadkill.Core.Converters.Markdown;
using Roadkill.Core.Converters.MediaWiki;
using System.Text.RegularExpressions;

namespace Roadkill.Core.Converters
{
	public class MarkupConverter
	{
		[ThreadStatic]
		private static IParser _parser;

		private static Regex _imgFileRegex = new Regex("^File:", RegexOptions.IgnoreCase);

		/// <summary>
		/// Turns the provided markup format into HTML.
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public string ToHtml(string text)
		{
			IParser parser = GetParser();
			return parser.Transform(text);
		}

		public IParser GetParser()
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
