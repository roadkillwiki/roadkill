using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Core.Plugins.Text.BuiltIn
{
	public class ClickableImages : TextPlugin
	{
		public override string Id
		{
			get 
			{
				return "ClickableImages";	
			}
		}

		public override string Name
		{
			get
			{
				return "Clickable images";
			}
		}

		public override string Description
		{
			get
			{
				return "Configures images so when they are clicked the source image is opened in a new window.";
			}
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		public ClickableImages()
		{
			AddScript("clickableimages.js");
		}

		public override string GetHeadContent()
		{
			return GetJavascriptHtml();
		}
	}
}