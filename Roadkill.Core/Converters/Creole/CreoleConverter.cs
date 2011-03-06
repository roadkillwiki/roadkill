using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters.Creole;
using System.Web.Mvc;
using System.Web;

namespace Roadkill.Core.Converters
{
	public class CreoleConverter : MarkupConverter
	{
		public override string ToHtml(string text)
		{
			UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);

			CreoleParser parser = new CreoleParser();
			parser.OnLink += delegate(object sender, LinkEventArgs e)
			{
				// This needs to be a lot more complete
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
					e.Src = helper.Content(RoadkillSettings.AttachmentsFolder+ "/" +e.OriginalSrc);
				}
			};
			return parser.ToHTML(text);
		}

		void parser_ImageProcessed(object sender, ImageProcessedEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}
