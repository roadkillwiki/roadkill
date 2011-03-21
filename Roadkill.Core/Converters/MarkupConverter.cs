using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters.Creole;
using System.Web.Mvc;
using System.Web;
using Roadkill.Core.Converters.Markdown;
using Roadkill.Core.Converters.MediaWiki;

namespace Roadkill.Core.Converters
{
	public class MarkupConverter
	{
		[ThreadStatic]
		private static IParser _parser;

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
				e.Src = helper.Content(RoadkillSettings.AttachmentsFolder + "/" + e.OriginalSrc);
			}
		}

		private void LinkParsed(object sender, LinkEventArgs e)
		{
			UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);

			// Process internal links
			if (!e.OriginalHref.StartsWith("http://") && !e.OriginalHref.StartsWith("www.") && !e.OriginalHref.StartsWith("mailto:"))
			{
				string href = e.OriginalHref;

				// Markdown doesn't support spaces in the URLs so "-" are used instead. Turn these back,
				// hopefully not creating any other issues in the process
				href = href.Replace("-", " ");

				PageManager manager = new PageManager();
				PageSummary summary = manager.FindByTitle(href);
				if (summary != null)
					href = helper.Action("Index", "Wiki", new { id = summary.Id,title=summary.Title.EncodeTitle() });

				e.Href = href;
				e.Target = "";
			}
		}
	}
}
