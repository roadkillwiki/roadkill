using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class SettingsService : ISettingsService
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly ApplicationSettings _applicationSettings;

		public SettingsService(IRepositoryFactory repositoryFactory, ApplicationSettings applicationSettings)
		{
			_repositoryFactory = repositoryFactory;
			_applicationSettings = applicationSettings;
		}

		public IEnumerable<RepositoryInfo> GetSupportedDatabases()
		{
			return _repositoryFactory.ListAll();
		}

		/// <summary>
		/// Retrieves the current site settings.
		/// </summary>
		/// <returns></returns>
		public SiteSettings GetSiteSettings()
		{
			var repository = _repositoryFactory.GetSettingsRepository(_applicationSettings.DatabaseName, _applicationSettings.ConnectionString);
			return repository.GetSiteSettings();
		}

		/// <summary>
		/// Saves all settings that are stored in the database, to the configuration table.
		/// </summary>
		/// <param name="model">Summary data containing the settings.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while saving the configuration.</exception>
		public void SaveSiteSettings(SettingsViewModel model)
		{
			try
			{
				SiteSettings siteSettings = new SiteSettings();
				siteSettings.AllowedFileTypes = model.AllowedFileTypes;
				siteSettings.AllowUserSignup = model.AllowUserSignup;
				siteSettings.IsRecaptchaEnabled = model.IsRecaptchaEnabled;
				siteSettings.MarkupType = model.MarkupType;
				siteSettings.RecaptchaPrivateKey = model.RecaptchaPrivateKey;
				siteSettings.RecaptchaPublicKey = model.RecaptchaPublicKey;
				siteSettings.SiteUrl = model.SiteUrl;
				siteSettings.SiteName = model.SiteName;
				siteSettings.Theme = model.Theme;

				// v2.0
				siteSettings.OverwriteExistingFiles = model.OverwriteExistingFiles;
				siteSettings.HeadContent = model.HeadContent;
				siteSettings.MenuMarkup = model.MenuMarkup;

				var repository = _repositoryFactory.GetSettingsRepository(_applicationSettings.DatabaseName, _applicationSettings.ConnectionString);
				repository.SaveSiteSettings(siteSettings);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while saving the site configuration.");
			}
		}
	}
}
