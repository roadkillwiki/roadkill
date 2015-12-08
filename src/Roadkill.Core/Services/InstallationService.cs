using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class InstallationService : IInstallationService
	{
		private readonly IRepositoryFactory _repositoryFactory;
		private readonly string _databaseName;
		private readonly string _connectionString;
		private readonly UserServiceBase _userService;

		public InstallationService(IRepositoryFactory repositoryFactory, string databaseName, string connectionString, UserServiceBase userService)
		{
			_repositoryFactory = repositoryFactory;
			_connectionString = connectionString;
			_userService = userService;
			_databaseName = databaseName;
		}

		public void AddAdminUser(string email, string password)
		{
			_userService.AddUser(email, "admin", password, true, false);
		}

		public IEnumerable<RepositoryInfo> GetSupportedDatabases()
		{
			return _repositoryFactory.ListAll();
		}

		/// <summary>
		/// Clears all users from the system.
		/// </summary>
		/// <exception cref="DatabaseException">An databaseerror occurred while clearing the user table.</exception>
		public void ClearUserTable()
		{
			try
			{
				var repository = _repositoryFactory.GetRepository(_databaseName, _connectionString);
				repository.DeleteAllUsers();
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while clearing the user tables.");
			}
		}

		/// <summary>
		/// Creates the database schema tables.
		/// </summary>
		/// <param name="model">The settings data.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while creating the database tables.</exception>
		public void CreateTables()
		{
			try
			{
				var repositoryInstaller = _repositoryFactory.GetRepositoryInstaller(_databaseName, _connectionString);
				repositoryInstaller.Install();
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while creating the site schema tables.");
			}
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

				var repository = _repositoryFactory.GetRepository(_databaseName, _connectionString);
				repository.SaveSiteSettings(siteSettings);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while saving the site configuration.");
			}
		}
	}
}
