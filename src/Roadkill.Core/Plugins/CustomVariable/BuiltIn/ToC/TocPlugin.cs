using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Plugins.BuiltIn.ToC
{
	public class TocPlugin : CustomVariablePlugin
	{
		public override string Id
		{
			get { return "ToC"; }
		}

		public override string Name
		{
			get { return "Table Of Contents"; }
		}

		public override string Description
		{
			get { return "Add a table of contents using the {{TOC}} tag"; }
		}

		public override string AfterParse(string html)
		{
			TocParser parser = new TocParser();
			html = parser.InsertToc(html);

			return html;
		}
	}
}
