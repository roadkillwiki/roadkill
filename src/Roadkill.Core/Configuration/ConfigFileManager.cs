using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Handles operations for the Roadkill .config file - both web.config, or custom configuration file.
	/// </summary>
	public class ConfigFileManager
	{
		private System.Configuration.Configuration _config;

		public ConfigFileManager(string configFilename = "")
		{
			if (string.IsNullOrEmpty(configFilename))
			{
				_config = WebConfigurationManager.OpenWebConfiguration("~");
			}
			else
			{
				if (configFilename.ToLower() == "app.config")
				{
					_config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
				}
				else
				{
					if (!File.Exists(configFilename))
						throw new FileNotFoundException(string.Format("The config file {0} could not be found", configFilename));

					ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
					fileMap.ExeConfigFilename = configFilename;
					_config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
				}
			}

			// If there's no Roadkill section, ConfigFileManager is in an invalid state
			RoadkillSection section = _config.GetSection("roadkill") as RoadkillSection;
			if (section == null)
				throw new InvalidOperationException(string.Format("The {0}config file{1} does not contain a Roadkill section",
													string.IsNullOrEmpty(configFilename) ? " web" : "",
													string.IsNullOrEmpty(configFilename) ? "" : " :" +configFilename));
		}

		/// <summary>
		/// Updates the 'version' attribute in the web.config to the current assembly version (for post-upgrades).
		/// </summary>
		public void WriteCurrentVersionToWebConfig()
		{
			try
			{
				RoadkillSection section = _config.GetSection("roadkill") as RoadkillSection;
				section.Version = ApplicationSettings.AssemblyVersion.ToString();
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new UpgradeException("An exception occurred while updating the version to the web.config", ex);
			}
		}

		public void Save()
		{
			_config.Save(ConfigurationSaveMode.Minimal);
		}

		/// <summary>
		/// Saves the relevant parts of <see cref="SettingsSummary"/> to the web.config.
		/// </summary>
		/// <param name="summary">Summary data containing the settings.</param>
		/// <exception cref="InstallerException">An error occurred writing to or saving the web.config file</exception>
		public void WriteSettings(SettingsSummary summary)
		{
			try
			{
				if (summary.UseWindowsAuth)
					WriteConfigForWindowsAuth();
				else
					WriteConfigForFormsAuth(summary);

				// Create a "Roadkill" connection string, or use the existing one if it exists.
				ConnectionStringSettings roadkillConnection = new ConnectionStringSettings("Roadkill", summary.ConnectionString);

				if (_config.ConnectionStrings.ConnectionStrings["Roadkill"] == null)
					_config.ConnectionStrings.ConnectionStrings.Add(roadkillConnection);
				else
					_config.ConnectionStrings.ConnectionStrings["Roadkill"].ConnectionString = summary.ConnectionString;

				// The roadkill section
				DataStoreType dataStoreType = DataStoreType.ByName(summary.DataStoreTypeName);
				RoadkillSection section = _config.GetSection("roadkill") as RoadkillSection;
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
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new InstallerException(ex, "An exception occurred while updating the settings to the web.config");
			}
		}

		/// <summary>
		/// Adds config settings for forms authentication.
		/// </summary>
		private void WriteConfigForFormsAuth(SettingsSummary summary)
		{
			// Turn on forms authentication
			AuthenticationSection authSection = _config.GetSection("system.web/authentication") as AuthenticationSection;
			authSection.Mode = AuthenticationMode.Forms;
			authSection.Forms.LoginUrl = "~/User/Login";

			// Turn on anonymous auth
			AnonymousIdentificationSection anonSection = _config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			anonSection.Enabled = true;
		}

		/// <summary>
		/// Adds web.config settings for windows authentication.
		/// </summary>
		private void WriteConfigForWindowsAuth()
		{
			// Turn on Windows authentication
			AuthenticationSection authSection = _config.GetSection("system.web/authentication") as AuthenticationSection;
			authSection.Forms.LoginUrl = "";
			authSection.Mode = AuthenticationMode.Windows;

			// Turn off anonymous auth
			AnonymousIdentificationSection anonSection = _config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			anonSection.Enabled = false;
		}

		/// <summary>
		/// Resets the roadkill "installed" property in the web.config for when the installation fails.
		/// </summary>
		/// <exception cref="InstallerException">An web.config related error occurred while reseting the install state.</exception>
		public void ResetInstalledState()
		{
			try
			{
				RoadkillSection section = _config.GetSection("roadkill") as RoadkillSection;
				section.Installed = false;
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new InstallerException(ex, "An exception occurred while resetting web.config install state to false.");
			}
		}

		/// <summary>
		/// Tests the web.config can be saved to by changing the "installed" to false.
		/// </summary>
		/// <returns>Any error messages or an empty string if no errors occurred.</returns>
		public string TestSaveWebConfig()
		{
			try
			{
				ResetInstalledState();
				Save();
				return "";
			}
			catch (Exception e)
			{
				return e.ToString();
			}
		}

		public System.Configuration.Configuration GetConfiguration()
		{
			return _config;
		}

		/// <summary>
		/// Loads the settings from the configuration file.
		/// </summary>
		/// <param name="config">The configuration to load the settings from. If this is null, the <see cref="ConfigurationManager"/> 
		/// is used to load the settings.</param>
		public ApplicationSettings GetApplicationSettings()
		{
			ApplicationSettings settings = new ApplicationSettings();

			RoadkillSection section = _config.GetSection("roadkill") as RoadkillSection;
			settings.AdminRoleName = section.AdminRoleName;
			settings.AttachmentsFolder = section.AttachmentsFolder;
			settings.AttachmentsRoutePath = section.AttachmentsRoutePath;
			settings.ConnectionStringName = section.ConnectionStringName;
			settings.ConnectionString = _config.ConnectionStrings.ConnectionStrings[section.ConnectionStringName].ConnectionString;
			if (string.IsNullOrEmpty(settings.ConnectionString))
				settings.ConnectionString = ConfigurationManager.ConnectionStrings[section.ConnectionStringName].ConnectionString;

			if (string.IsNullOrEmpty(settings.ConnectionString))
				Log.Warn("ConnectionString property is null/empty.");

			// Ignore the legacy useCache and cacheText section keys, as the behaviour has changed.
			settings.UseObjectCache = section.UseObjectCache;
			settings.UseBrowserCache = section.UseBrowserCache;

			// Look for the legacy database type key
			string dataStoreType = section.DataStoreType;
			if (string.IsNullOrEmpty(dataStoreType) && !string.IsNullOrEmpty(section.DatabaseType))
				dataStoreType = section.DatabaseType;

			LogType loggingType;
			if (!Enum.TryParse<LogType>(section.Logging, true, out loggingType))
				loggingType = LogType.None;

			settings.LoggingType = loggingType;
			settings.LogErrorsOnly = section.LogErrorsOnly;
			settings.DataStoreType = DataStoreType.ByName(dataStoreType);
			settings.ConnectionStringName = section.ConnectionStringName;
			settings.EditorRoleName = section.EditorRoleName;
			settings.IgnoreSearchIndexErrors = section.IgnoreSearchIndexErrors;
			settings.IsPublicSite = section.IsPublicSite;
			settings.Installed = section.Installed;
			settings.LdapConnectionString = section.LdapConnectionString;
			settings.LdapUsername = section.LdapUsername;
			settings.LdapPassword = section.LdapPassword;
			settings.RepositoryType = section.RepositoryType;
			settings.ResizeImages = section.ResizeImages;
			settings.UseHtmlWhiteList = section.UseHtmlWhiteList;
			settings.UserManagerType = section.UserManagerType;
			settings.UseWindowsAuthentication = section.UseWindowsAuthentication;

			if (string.IsNullOrEmpty(section.Version))
			{
				settings.UpgradeRequired = true;
			}
			else
			{
				Version configVersion = null;
				if (Version.TryParse(section.Version, out configVersion))
				{
					settings.UpgradeRequired = (configVersion != ApplicationSettings.AssemblyVersion);
				}
				else
				{
					Log.Warn("Invalid Version found ({0}) in the web.config, assuming it's the same as the assembly version ({1})", section.Version, ApplicationSettings.AssemblyVersion);
					settings.UpgradeRequired = false;
				}
			}

			return settings;
		}
	}
}
