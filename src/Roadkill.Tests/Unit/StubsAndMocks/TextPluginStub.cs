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
		private string _id;
		private string _name;
		private string _description;

		public override string Id { get { return _id; } }
		public override string Name { get { return _name; } }
		public override string Description { get { return _description; } }

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		public TextPluginStub() : this(null, null) { }

		public TextPluginStub(ApplicationSettings applicationSettings, IRepository repository)
			: base(applicationSettings, repository)
		{
			_id = "Amazing plugin";
			_name = "An amazing plugin";
			_description = "Amazing stubbed plugin";
		}

		public TextPluginStub(string id, string name, string description) : base(null, null)
		{
			_id = id;
			_name = name;
			_description = description;
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
		