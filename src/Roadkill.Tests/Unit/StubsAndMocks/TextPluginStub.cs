using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class TextPluginStub : TextPlugin
	{
		public override string Id
		{
			get { return "AmazingPlugin"; }
		}

		public override string Name
		{
			get { return "An Amazing Plugin"; }
		}

		public override string Description
		{
			get { return "Description"; }
		}

		public TextPluginStub() : this(null, null) { }

		public TextPluginStub(ApplicationSettings applicationSettings, IRepository repository)
			: base(applicationSettings, repository)
		{
		}

		public override string BeforeParse(string markupText)
		{
			return markupText.Replace("~~~usertoken~~~", "<span>usertoken</span>");
		}

		public override string AfterParse(string html)
		{
			return html.Replace("<strong>", "<strong style='color:green'><iframe src='javascript:alert(test)'>");
		}
	}
}
