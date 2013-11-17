using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class TextPluginStub : TextPlugin
	{
		private string _id;
		private string _name;
		private string _description;
		private string _version;

		public override string Id { get { return _id; } }
		public override string Name { get { return _name; } }
		public override string Description { get { return _description; } }
		public string HeadContent { get; set; }
		public string FooterContent { get; set; }
		public string PreContainerHtml { get; set; }
		public string PostContainerHtml { get; set; }

		public override string Version
		{

			get
			{
				return _version;
			}
		}

		public TextPluginStub()
		{
			_id = "Amazing plugin";
			_name = "An amazing plugin";
			_description = "Amazing stubbed plugin";
		}

		internal TextPluginStub(IRepository repository, SiteCache siteCache) : base(repository, siteCache)
		{
			_id = "Amazing plugin";
			_name = "An amazing plugin";
			_description = "Amazing stubbed plugin";
		}

		public TextPluginStub(string id, string name, string description, string version = "1.0")
		{
			_id = id;
			_name = name;
			_description = description;
			_version = version;
		}

		public override void OnInitializeSettings(PluginSettings settings)
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

		public override string GetHeadContent()
		{
			return HeadContent;
		}

		public override string GetFooterContent()
		{
			return FooterContent;
		}

		public override string GetPreContainerHtml()
		{
			return PreContainerHtml;
		}

		public override string GetPostContainerHtml()
		{
			return PostContainerHtml;
		}
	}
}
		