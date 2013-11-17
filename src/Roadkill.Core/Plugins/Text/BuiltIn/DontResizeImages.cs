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
	/// <summary>
	/// Sets images (via CSS) so they don't get scaled by Bootstrap and are their original size.
	/// </summary>
	public class DontResizeImages : TextPlugin
	{
		public override string Id
		{
			get 
			{ 
				return "DontResizeImages";	
			}
		}

		public override string Name
		{
			get
			{
				return "Dont't resize images";
			}
		}

		public override string Description
		{
			get
			{
				return "Ensure all images are always their original width/height and not sized to fit the page.";
			}
		}

		public override string Version
		{

			get
			{
				return "1.0";
			}
		}

		public override string GetHeadContent()
		{
			return GetCssLink("dontresizeimages.css");
		}
	}
}