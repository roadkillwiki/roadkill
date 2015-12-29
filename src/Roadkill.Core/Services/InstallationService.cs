using System;
using System.Collections.Generic;
using System.Web.Routing;
using Microsoft.Practices.ServiceLocation;
using Mindscape.LightSpeed;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Schema;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class InstallationService : IInstallationService
	{
		private Func<string, string, IInstallerRepository> _getRepositoryFunc;
		internal ServiceLocatorImplBase Locator { get; set; }

		public InstallationService()
		{
			_getRepositoryFunc = GetRepository;
			Locator = LocatorStartup.Locator;
		}

		internal InstallationService(Func<string, string, IInstallerRepository> getRepositoryFunc, ServiceLocatorImplBase locator)
		{
			_getRepositoryFunc = getRepositoryFunc;
			Locator = locator;
		}

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
				IInstallerRepository installerRepository = _getRepositoryFunc(model.DatabaseName, model.ConnectionString);
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

				// Attachments handler needs re-registering
				var appSettings = Locator.GetInstance<ApplicationSettings>();
				var fileService = Locator.GetInstance<IFileService>();
				AttachmentRouteHandler.RegisterRoute(appSettings, RouteTable.Routes, fileService);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while saving the site configuration.");
			}
		}

		private IInstallerRepository GetRepository(string databaseName, string connectionString)
		{
			if (databaseName == SupportedDatabases.MongoDB)
			{
				return new MongoDbInstallerRepository(connectionString);
			}
			else if (databaseName == SupportedDatabases.MySQL)
			{
				return new LightSpeedInstallerRepository(DataProvider.MySql5, new MySqlSchema(), connectionString);
			}
			else if (databaseName == SupportedDatabases.Postgres)
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
