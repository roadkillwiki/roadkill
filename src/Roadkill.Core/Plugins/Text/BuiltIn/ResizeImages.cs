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
	/// Sets images (via CSS) so they are no bigger than the page.
	/// </summary>
	public class ResizeImages : TextPlugin
	{
		public override string Id
		{
			get 
			{
				return "ResizeImages";	
			}
		}

		public override string Name
		{
			get
			{
				return "Resize images";
			}
		}

		public override string Description
		{
			get
			{
				return "Ensure all images are always fit the page.";
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
			return GetCssLink("resizeimages.css");
		}
	}
}