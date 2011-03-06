using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters.MediaWiki;

namespace Roadkill.Core.Converters
{
	public class MediaWikiConverter : MarkupConverter
	{
		public override string ToHtml(string text)
		{
			MediaWikiParser parser = new MediaWikiParser();
			return parser.ToHTML(text);
		}
	}
}
