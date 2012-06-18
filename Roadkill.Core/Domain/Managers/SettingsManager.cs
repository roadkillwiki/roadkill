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
using NHibernate;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides common tasks for changing the Roadkill application settings.
	/// </summary>
	public class SettingsManager
	{
		/// <summary>
		/// Clears all pages and page content from the database.
		/// </summary>
		/// <exception cref="DatabaseException">An NHibernate (database) error occured while clearing the page data.</exception>
		public static void ClearPageTables()
		{
			try
			{
				NHibernateRepository.Current.DeleteAll<PageContent>();
				NHibernateRepository.Current.DeleteAll<Page>();
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An exception occured while clearing all page tables.");
			}
		}

		/// <summary>
		/// Clears all users from the system.
		/// </summary>
		/// <exception cref="DatabaseException">An NHibernate (database) error occured while clearing the user table.</exception>
		public static void ClearUserTable()
		{
			try
			{
				NHibernateRepository.Current.DeleteAll<User>();
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An exception occured while clearing the user tables.");
			}
		}

		/// <summary>
		/// Creates the database schema tables.
		/// </summary>
		/// <param name="summary">The settings data.</param>
		/// <exception cref="DatabaseException">An NHibernate (database) error occured while creating the database tables.</exception>
		public static void CreateTables(SettingsSummary summary)
		{
			try
			{
				NHibernateRepository.Current.Configure(RoadkillSettings.DatabaseType, summary.ConnectionString, true, summary.CacheEnabled);
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An exception occured while creating the site schema tables.");
			}
		}

		/// <summary>
		/// Saves all settings that are stored in the database, to the configuration table.
		/// </summary>
		/// <param name="summary">Summary data containing the settings.</param>
		/// <param name="isInstalling">If true, a new <see cref="SiteConfiguration"/> is created, otherwise the current one is updated.</param>
		/// <exception cref="DatabaseException">An NHibernate (database) error occured while saving the configuration.</exception>
		public static void SaveSiteConfiguration(SettingsSummary summary, bool isInstalling)
		{
			try
			{
				SiteConfiguration config;

				if (isInstalling)
				{
					config = new SiteConfiguration();
				}
				else
				{
					config = SiteConfiguration.Current;
				}

				config.AllowedFileTypes = summary.AllowedExtensions;
				config.AllowUserSignup = summary.AllowUserSignup;
				config.EnableRecaptcha = summary.EnableRecaptcha;
				config.MarkupType = summary.MarkupType;
				config.RecaptchaPrivateKey = summary.RecaptchaPrivateKey;
				config.RecaptchaPublicKey = summary.RecaptchaPublicKey;
				config.SiteUrl = summary.SiteUrl;
				config.Title = summary.SiteName;
				config.Theme = summary.Theme;
				
				config.Version = RoadkillSettings.Version;

				NHibernateRepository.Current.SaveOrUpdate<SiteConfiguration>(config);
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An exception occured while saving the site configuration.");
			}
		}

		/// <summary>
		/// Saves the relevant parts of <see cref="SettingsSummary"/> to the web.config.
		/// </summary>
		/// <param name="summary">Summary data containing the settings.</param>
		/// <exception cref="InstallerException">An error occured writing to or saving the web.config file</exception>
		public static void SaveWebConfigSettings(SettingsSummary summary)
		{
			try
			{
				Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

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
				RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;
				section.AdminRoleName = summary.AdminRoleName;
				section.AttachmentsFolder = summary.AttachmentsFolder;
				section.CacheEnabled = summary.CacheEnabled;
				section.CacheText = summary.CacheText;
				section.ConnectionStringName = "Roadkill";
				section.DatabaseType = summary.DatabaseType.ToString();
				section.EditorRoleName = summary.EditorRoleName;
				section.LdapConnectionString = summary.LdapConnectionString;
				section.LdapUsername = summary.LdapUsername;
				section.LdapPassword = summary.LdapPassword;
				section.UseWindowsAuthentication = summary.UseWindowsAuth;

				section.Installed = true;

				config.Save(ConfigurationSaveMode.Minimal);
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new InstallerException(ex, "An exception occured while updating the settings to the web.config");
			}
		}

		/// <summary>
		/// Adds web.config settings for windows authentication.
		/// </summary>
		private static void WriteConfigForWindowsAuth(Configuration config)
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
		private static void WriteConfigForFormsAuth(Configuration config, SettingsSummary summary)
		{
			// Turn on forms authentication
			AuthenticationSection authSection = config.GetSection("system.web/authentication") as AuthenticationSection;
			authSection.Mode = AuthenticationMode.Forms;
			authSection.Forms.LoginUrl = "~/User/Login";

			// Turn on anonymous auth
			AnonymousIdentificationSection anonSection = config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			anonSection.Enabled = true;
		}
	}
}
