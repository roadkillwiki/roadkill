using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Services;
using StructureMap;
using StructureMap.Attributes;

namespace Roadkill.Core.Mvc.WebViewPages
{
	public abstract class RoadkillViewPage<T> : WebViewPage<T>
	{
		// Constructor injection isn't viable here, as this class is created by the ASP.NET runtime
		private SiteSettings _siteSettings;

		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }
		
		[SetterProperty]
		public IUserContext RoadkillContext { get; set; }
		
		[SetterProperty]
		public MarkupConverter MarkupConverter { get; set; }

		[SetterProperty]
		public SettingsService SettingsService { get; set; }

		public SiteSettings SiteSettings
		{
			get
			{
				if (_siteSettings == null)
					_siteSettings = SettingsService.GetSiteSettings();

				return _siteSettings;
			}
		}
	}
}
