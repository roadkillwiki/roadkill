using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Web.Configuration;
using System.DirectoryServices;
using System.Web.Management;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class SettingsManager : ServiceBase
	{
		public SettingsManager(IConfigurationContainer configuration, IRepository repository)
			: base(configuration, repository)
		{
		}

		/// <summary>
		/// Clears all pages and page content from the database.
		/// </summary>
		/// <exception cref="DatabaseException">An datastore error occurred while clearing the page data.</exception>
		public void ClearPageTables()
		{
			try
			{
				Repository.DeleteAllPageContent();
				Repository.DeleteAllPages();
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while clearing all page tables.");
			}
		}

		/// <summary>
		/// Clears all users from the system.
		/// </summary>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while clearing the user table.</exception>
		public void ClearUserTable()
		{
			try
			{
				Repository.DeleteAllUsers();
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while clearing the user tables.");
			}
		}

		/// <summary>
		/// Creates the database schema tables.
		/// </summary>
		/// <param name="summary">The settings data.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while creating the database tables.</exception>
		public void CreateTables(SettingsSummary summary)
		{
			try
			{
				DataStoreType dataStoreType = DataStoreType.ByName(summary.DataStoreTypeName);
				Repository.Install(dataStoreType, summary.ConnectionString, summary.UseObjectCache);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while creating the site schema tables.");
			}
		}

		/// <summary>
		/// Saves all settings that are stored in the database, to the configuration table.
		/// </summary>
		/// <param name="summary">Summary data containing the settings.</param>
		/// <param name="isInstalling">If true, a new <see cref="SitePreferences"/> is created, otherwise the current one is updated.</param>
		/// <exception cref="DatabaseException">An datastore error occurred while saving the configuration.</exception>
		public void SaveSitePreferences(SettingsSummary summary, bool isInstalling)
		{
			try
			{
				SitePreferences config = new SitePreferences();
				config.AllowedFileTypes = summary.AllowedExtensions;
				config.AllowUserSignup = summary.AllowUserSignup;
				config.IsRecaptchaEnabled = summary.EnableRecaptcha;
				config.MarkupType = summary.MarkupType;
				config.RecaptchaPrivateKey = summary.RecaptchaPrivateKey;
				config.RecaptchaPublicKey = summary.RecaptchaPublicKey;
				config.SiteUrl = summary.SiteUrl;
				config.SiteName = summary.SiteName;
				config.Theme = summary.Theme;

				Repository.SaveSitePreferences(config);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while saving the site configuration.");
			}
		}

		/// <summary>
		/// Saves the relevant parts of <see cref="SettingsSummary"/> to the web.config.
		/// </summary>
		/// <param name="summary">Summary data containing the settings.</param>
		/// <exception cref="InstallerException">An error occurred writing to or saving the web.config file</exception>
		public void SaveWebConfigSettings(SettingsSummary summary)
		{
			try
			{
				System.Configuration.Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

				if (summary.UseWindowsAuth)
				{
					WriteConfigForWindowsAuth(config);
				}
				else
				{
					WriteConfigForFormsAuth(config, summary);
				}

				// Create a "Roadkill" connection string, or use the existing one if it exists.
				ConnectionStringSettings roadkillConnection = new ConnectionStringSettings("Roadkill", summary.ConnectionString);

				if (config.ConnectionStrings.ConnectionStrings["Roadkill"] == null)
					config.ConnectionStrings.ConnectionStrings.Add(roadkillConnection);
				else
					config.ConnectionStrings.ConnectionStrings["Roadkill"].ConnectionString = summary.ConnectionString;

				// The roadkill section
				DataStoreType dataStoreType = DataStoreType.ByName(summary.DataStoreTypeName);
				RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;
				section.AdminRoleName = summary.AdminRoleName;
				section.AttachmentsFolder = summary.AttachmentsFolder;
				section.UseObjectCache = summary.UseObjectCache;
				section.UseBrowserCache = summary.UseBrowserCache;
				section.ConnectionStringName = "Roadkill";
				section.DataStoreType = dataStoreType.Name;
				section.EditorRoleName = summary.EditorRoleName;
				section.LdapConnectionString = summary.LdapConnectionString;
				section.LdapUsername = summary.LdapUsername;
				section.LdapPassword = summary.LdapPassword;
				section.RepositoryType = dataStoreType.CustomRepositoryType;
				section.UseWindowsAuthentication = summary.UseWindowsAuth;
				
				// Optional "tweak" settings - these need to be explicit as DefaultValue="" in the attribute doesn't determine the value when saving.
				section.IsPublicSite = true;
				section.IgnoreSearchIndexErrors = true;
				section.ResizeImages = true;
				section.Version = ApplicationSettings.AssemblyVersion.ToString();

				section.Installed = true;

				config.Save(ConfigurationSaveMode.Minimal);
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new InstallerException(ex, "An exception occurred while updating the settings to the web.config");
			}
		}

		/// <summary>
		/// Adds web.config settings for windows authentication.
		/// </summary>
		private void WriteConfigForWindowsAuth(System.Configuration.Configuration config)
		{
			// Turn on Windows authentication
			AuthenticationSection authSection = config.GetSection("system.web/authentication") as AuthenticationSection;
			authSection.Forms.LoginUrl = "";
			authSection.Mode = AuthenticationMode.Windows;

			// Turn off anonymous auth
			AnonymousIdentificationSection anonSection = config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			anonSection.Enabled = false;
		}

		/// <summary>
		/// Adds web.config settings for forms authentication.
		/// </summary>
		private void WriteConfigForFormsAuth(System.Configuration.Configuration config, SettingsSummary summary)
		{
			// Turn on forms authentication
			AuthenticationSection authSection = config.GetSection("system.web/authentication") as AuthenticationSection;
			authSection.Mode = AuthenticationMode.Forms;
			authSection.Forms.LoginUrl = "~/User/Login";

			// Turn on anonymous auth
			AnonymousIdentificationSection anonSection = config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			anonSection.Enabled = true;
		}

		/// <summary>
		/// Updates the 'version' attribute in the web.config to the current assembly version (for post-upgrades).
		/// </summary>
		public void SaveCurrentVersionToWebConfig()
		{
			try
			{
				System.Configuration.Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

				RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;
				section.Version = ApplicationSettings.AssemblyVersion.ToString();

				config.Save(ConfigurationSaveMode.Minimal);
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new UpgradeException("An exception occurred while updating the version to the web.config", ex);
			}
		}
	}
}
