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

		private IParser GetParser()
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

			// This needs to be a lot more complete
			if (!e.OriginalHref.StartsWith("http://") && !e.OriginalHref.StartsWith("www.") && !e.OriginalHref.StartsWith("mailto:"))
			{
				e.Href = helper.Action("Index", "Page", new { id = e.OriginalHref });
				e.Target = "";
			}
		}
	}
}
