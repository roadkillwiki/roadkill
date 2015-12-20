using System;
using System.Collections.Generic;
using Mindscape.LightSpeed;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Schema;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class InstallationService : IInstallationService
	{
		public IEnumerable<RepositoryInfo> GetSupportedDatabases()
		{
			return new List<RepositoryInfo>()
			{
				SupportedDatabases.MongoDB,
				SupportedDatabases.MySQL,
				SupportedDatabases.Postgres,
				SupportedDatabases.SqlServer2008
			};
		}

		public void Install(SettingsViewModel model)
		{
			try
			{
				IInstallerRepository installerRepository = GetRepository(model.DatabaseName, model.ConnectionString);
				installerRepository.CreateSchema();

				if (model.UseWindowsAuth == false)
				{
					installerRepository.AddAdminUser(model.AdminEmail, "admin", model.AdminPassword);
				}

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

		private IInstallerRepository GetRepository(string databaseProvider, string connectionString)
		{
			if (databaseProvider == SupportedDatabases.MongoDB)
			{
				return new MongoDbInstallerRepository(connectionString);
			}
			else if (databaseProvider == SupportedDatabases.MySQL)
			{
				return new LightSpeedInstallerRepository(DataProvider.MySql5, new MySqlSchema(), connectionString);
			}
			else if (databaseProvider == SupportedDatabases.Postgres)
			{
				return new LightSpeedInstallerRepository(DataProvider.PostgreSql9, new PostgresSchema(), connectionString);
			}
			else
			{
				return new LightSpeedInstallerRepository(DataProvider.SqlServer2008, new SqlServerSchema(), connectionString);
			}
		}
	}
}
