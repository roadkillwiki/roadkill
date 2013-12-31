using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using Ionic.Zip;
using Roadkill.Core.Localization;
using Roadkill.Core.Configuration;
using Roadkill.Core.Cache;
using Roadkill.Core.Services;
using Roadkill.Core.Import;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Logging;
using Roadkill.Core.Database.Export;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;
using Roadkill.Core.Attachments;
using Roadkill.Core.DI;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality for the settings page including tools and user management.
	/// </summary>
	/// <remarks>All actions in this controller require admin rights.</remarks>
	[AdminRequired]
	public class SettingsController : ControllerBase
	{
		private SettingsService _settingsService;
		private SiteCache _siteCache;
		private ConfigReaderWriter _configReaderWriter;

		public SettingsController(ApplicationSettings settings, UserServiceBase userManager, SettingsService settingsService, 
			IUserContext context, SiteCache siteCache, ConfigReaderWriter configReaderWriter)
			: base(settings, userManager, context, settingsService) 
		{
			_settingsService = settingsService;
			_siteCache = siteCache;
			_configReaderWriter = configReaderWriter;
		}

		/// <summary>
		/// The default settings page that displays the current Roadkill settings.
		/// </summary>
		/// <returns>A <see cref="SettingsViewModel"/> as the model.</returns>
		public ActionResult Index()
		{
			SiteSettings siteSettings = SettingsService.GetSiteSettings();
			SettingsViewModel model = new SettingsViewModel(ApplicationSettings, siteSettings);

			return View(model);
		}

		/// <summary>
		/// Saves the <see cref="SettingsViewModel"/> that is POST'd to the action.
		/// </summary>
		/// <param name="model">The settings to save to the web.config/database.</param>
		/// <returns>A <see cref="SettingsViewModel"/> as the model.</returns>
		[HttpPost]
		[ValidateInput(false)]
		public ActionResult Index(SettingsViewModel model)
		{
			if (ModelState.IsValid)
			{
				_configReaderWriter.Save(model);
			
				_settingsService.SaveSiteSettings(model);
				_siteCache.RemoveMenuCacheItems();

				// Refresh the AttachmentsDirectoryPath using the absolute attachments path, as it's calculated in the constructor
				ApplicationSettings appSettings = _configReaderWriter.GetApplicationSettings();
				model.FillFromApplicationSettings(appSettings);
				model.UpdateSuccessful = true;
			}

			return View(model);
		}
	}
}
