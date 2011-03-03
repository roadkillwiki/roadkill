using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters.Markdown;

namespace Roadkill.Core.Converters
{
	public class MarkdownConverter : MarkupConverterBase
	{
		public override string ToHtml(string text)
		{
			MarkdownParser parser = new MarkdownParser();
			return parser.Transform(text);
		}
	}
}
