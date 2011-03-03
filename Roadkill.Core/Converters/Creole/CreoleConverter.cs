using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Converters.Creole;

namespace Roadkill.Core.Converters
{
	public class CreoleConverter : MarkupConverterBase
	{
		public override string ToHtml(string text)
		{
			CreoleParser parser = new CreoleParser();
			return parser.ToHTML(text);
		}
	}
}
