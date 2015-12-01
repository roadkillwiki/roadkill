using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.Services;
using StructureMap;
using StructureMap.Attributes;

namespace Roadkill.Core.Mvc.WebViewPages
{
	// Layout pages aren't created using IDependencyResolver (as they're outside of MVC). So use bastard injection for them.
	public abstract class RoadkillLayoutPage : WebViewPage<object>
	{
		public ApplicationSettings ApplicationSettings { get; set; }
		public IUserContext RoadkillContext { get; set; }
		public MarkupConverter MarkupConverter { get; set; }
		public SiteSettings SiteSettings { get; set; }

		public RoadkillLayoutPage()
		{
			ApplicationSettings = DependencyResolution.LocatorStartup.Locator.GetInstance<ApplicationSettings>();
			RoadkillContext = DependencyResolution.LocatorStartup.Locator.GetInstance<IUserContext>();

			if (ApplicationSettings.Installed && !ApplicationSettings.UpgradeRequired)
			{
				MarkupConverter = DependencyResolution.LocatorStartup.Locator.GetInstance<MarkupConverter>();
				SiteSettings = DependencyResolution.LocatorStartup.Locator.GetInstance<SettingsService>().GetSiteSettings();
			}
		}
	}
}
