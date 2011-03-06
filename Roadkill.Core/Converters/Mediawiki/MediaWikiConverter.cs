using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters.MediaWiki;
using System.Web.Mvc;
using Roadkill.Core.Converters.Creole;
using System.Web;

namespace Roadkill.Core.Converters
{
	public class MediaWikiConverter : MarkupConverter
	{
		public override string ToHtml(string text)
		{
			UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);

			MediaWikiParser parser = new MediaWikiParser();
			parser.OnLink += delegate(object sender, LinkEventArgs e)
			{
				if (!e.Link.StartsWith("http://") && !e.Link.StartsWith("www.") && !e.Link.StartsWith("mailto:"))
				{
					e.Href = helper.Action("Index", "Page", new { id = e.Link });
					e.Target = LinkEventArgs.TargetEnum.Internal;
				}
			};
			parser.ImageProcessed += delegate(object sender, ImageProcessedEventArgs e)
			{
				if (!e.OriginalSrc.StartsWith("http://") && !e.OriginalSrc.StartsWith("www."))
				{
					e.Src = helper.Content(RoadkillSettings.AttachmentsFolder + "/" + e.OriginalSrc);
				}
			};
			return parser.ToHTML(text);
		}
	}
}
