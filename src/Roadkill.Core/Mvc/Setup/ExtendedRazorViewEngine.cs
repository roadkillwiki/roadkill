using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Roadkill.Core.Mvc
{
	public class ExtendedRazorViewEngine : RazorViewEngine
	{
		public static void Register()
		{
			// Add a search path for /Dialogs, /Plugins/SpecialPages via a custom view engine.
			ViewEngines.Engines.Clear();

			// {1} is the controller, {0} is the action
			ExtendedRazorViewEngine engine = new ExtendedRazorViewEngine();
			engine.AddPartialViewLocationFormat("~/Views/Shared/Dialogs/{0}.cshtml");
			engine.AddViewLocationFormat("~/Plugins/{0}.cshtml");
			engine.AddPartialViewLocationFormat("~/Plugins/{0}.cshtml");

			ViewEngines.Engines.Add(engine);
		}

		public void AddViewLocationFormat(string paths)
		{
			List<string> existingPaths = new List<string>(ViewLocationFormats);
			existingPaths.Add(paths);

			ViewLocationFormats = existingPaths.ToArray();
		}

		public void AddPartialViewLocationFormat(string paths)
		{
			List<string> existingPaths = new List<string>(PartialViewLocationFormats);
			existingPaths.Add(paths);

			PartialViewLocationFormats = existingPaths.ToArray();
		}
	}
}
