using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Core.Plugins.BuiltIn
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

		public ExternalLinksInNewWindow(ApplicationSettings applicationSettings, IRepository repository)
			: base(applicationSettings, repository)
		{
		}

		public override string GetHeadContent()
		{
			AddScriptWithHeadJS("externallinksinnewwindow.js");
			return GetScriptHtmlWithHeadJS();
		}
	}
}