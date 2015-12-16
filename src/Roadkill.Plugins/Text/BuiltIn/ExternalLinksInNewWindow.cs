using Roadkill.Core.Plugins;

namespace Roadkill.Plugins.Text.BuiltIn
{
	public class ExternalLinksInNewWindow : TextPlugin
	{
		public override string Id
		{
			get 
			{ 
				return "ExternalLinksInNewWindow";	
			}
		}

		public override string Name
		{
			get
			{
				return "External links in new window";
			}
		}

		public override string Description
		{
			get
			{
				return "Configures all external links to open in a new window/tab.";
			}
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		public ExternalLinksInNewWindow()
		{
			AddScript("externallinksinnewwindow.js");
		}

		public override string GetHeadContent()
		{
			return GetJavascriptHtml();
		}
	}
}