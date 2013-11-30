using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Core.Plugins.Text.BuiltIn.ToC
{
	public class TocPlugin : TextPlugin
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
			get { return "Add a table of contents using the {TOC} tag"; }
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		public TocPlugin()
		{
			AddScript("toc.js");
		}

		public override string AfterParse(string html)
		{
			TocParser parser = new TocParser();
			html = parser.InsertToc(html);

			return html;
		}

		public override string GetHeadContent()
		{
			string html = GetJavascriptHtml();
			html += GetCssLink("toc.css");

			return html;
		}
	}
}
