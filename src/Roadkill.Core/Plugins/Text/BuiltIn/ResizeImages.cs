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
				return "ResizeImages page";
			}
		}

		public override string Description
		{
			get
			{
				return "ResizeImages";
			}
		}

		public ResizeImages(ApplicationSettings applicationSettings, IRepository repository)
			: base(applicationSettings, repository)
		{
		}
		
		public override string GetHeadContent()
		{
			return GetCssLink("resizeimages.css");
		}
	}
}