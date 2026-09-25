using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.WebViewPages
{
	/// <summary>
	/// The base class for all Roadkill Razor views. The Roadkill services are resolved (lazily) from the request's services.
	/// </summary>
	public abstract class RoadkillViewPage<T> : RazorPage<T>
	{
		private ApplicationSettings _applicationSettings;
		private IUserContext _roadkillContext;
		private MarkupConverter _markupConverter;
		private SettingsService _settingsService;
		private SiteSettings _siteSettings;

		public ApplicationSettings ApplicationSettings
		{
			get { return _applicationSettings ?? (_applicationSettings = Context.RequestServices.GetRequiredService<ApplicationSettings>()); }
			set { _applicationSettings = value; }
		}

		public IUserContext RoadkillContext
		{
			get { return _roadkillContext ?? (_roadkillContext = Context.RequestServices.GetRequiredService<IUserContext>()); }
			set { _roadkillContext = value; }
		}

		public MarkupConverter MarkupConverter
		{
			get { return _markupConverter ?? (_markupConverter = Context.RequestServices.GetRequiredService<MarkupConverter>()); }
			set { _markupConverter = value; }
		}

		public SettingsService SettingsService
		{
			get { return _settingsService ?? (_settingsService = Context.RequestServices.GetRequiredService<SettingsService>()); }
			set { _settingsService = value; }
		}

		public SiteSettings SiteSettings
		{
			get
			{
				if (_siteSettings == null)
					_siteSettings = ApplicationSettings.Installed ? SettingsService.GetSiteSettings() : new SiteSettings();

				return _siteSettings;
			}
		}
	}
}
