using System;
using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class InstallationService : IInstallationService
	{
		private readonly Func<string, string, IInstallerRepository> _getRepositoryFunc;
		private readonly IRepositoryFactory _repositoryFactory;

		public InstallationService(IRepositoryFactory repositoryFactory)
		{
			_repositoryFactory = repositoryFactory;
			_getRepositoryFunc = GetRepository;
		}

		internal InstallationService(Func<string, string, IInstallerRepository> getRepositoryFunc)
		{
			_repositoryFactory = new RepositoryFactory();
			_getRepositoryFunc = getRepositoryFunc;
		}

		public IEnumerable<RepositoryInfo> GetSupportedDatabases()
		{
			return _repositoryFactory.ListAll();
		}

		public void Install(SettingsViewModel model)
		{
			try
			{
				IInstallerRepository installerRepository = _getRepositoryFunc(model.DatabaseName, model.ConnectionString);
				installerRepository.CreateSchema();
				installerRepository.AddAdminUser(model.AdminEmail, "admin", model.AdminPassword);

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
				installerRepository.SaveSettings(siteSettings);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while saving the site configuration.");
			}
		}

		internal IInstallerRepository GetRepository(string databaseName, string connectionString)
		{
			return _repositoryFactory.GetInstallerRepository(databaseName, connectionString);
		}
	}
}
