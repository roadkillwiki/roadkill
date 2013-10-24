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
	/// <summary>
	/// Whether to scale images dynamically on the page, using Javascript, so they fit inside the main page container (400x400px).
	/// </summary>
	public class ResizeLargeImages : TextPlugin
	{
		public override string Id
		{
			get 
			{ 
				return "ResizeLargeImages";	
			}
		}

		public override string Name
		{
			get
			{
				return "Resize large images";
			}
		}

		public override string Description
		{
			get
			{
				return "Resizes large images using CSS so they always fit the page width/height.";
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
			return GetCssLink("resizelargeimages.css");
		}
	}
}