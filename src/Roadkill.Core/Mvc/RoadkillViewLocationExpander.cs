using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Roadkill.Core.Mvc
{
	/// <summary>
	/// Adds the Roadkill view search paths: the dialogs folder and the plugins folder (previously the ExtendedRazorViewEngine).
	/// </summary>
	public class RoadkillViewLocationExpander : IViewLocationExpander
	{
		public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
		{
			// {1} is the controller, {0} is the action
			return viewLocations.Concat(new[]
			{
				"/Views/Shared/Dialogs/{0}.cshtml",
				"/Plugins/{0}.cshtml"
			});
		}

		public void PopulateValues(ViewLocationExpanderContext context)
		{
		}
	}
}
