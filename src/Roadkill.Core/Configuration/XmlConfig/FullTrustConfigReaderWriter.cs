using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Xml.Linq;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Configuration
{
	public class FullTrustConfigReaderWriter : ConfigReaderWriter
	{
		private System.Configuration.Configuration _config;
		private bool _isWebConfig;
		private bool _isConfigLoaded;
		private RoadkillSection _section;

		public FullTrustConfigReaderWriter(string configFilePath) : base(configFilePath)
		{
			if (!_isConfigLoaded)
			{
				if (string.IsNullOrEmpty(configFilePath))
				{
					_config = WebConfigurationManager.OpenWebConfiguration("~");
					_isWebConfig = true;
				}
				else
				{
					if (!File.Exists(configFilePath))
						throw new ConfigurationException(null, "The XML config file {0} could not be found", configFilePath);

					if (configFilePath.ToLower() == "app.config")
					{
						_config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
					}
					else
					{
						if (!File.Exists(configFilePath))
							throw new FileNotFoundException(string.Format("The config file {0} could not be found", configFilePath));

						ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
						fileMap.ExeConfigFilename = configFilePath;
						_config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
					}

					_isWebConfig = false;
				}

				ConfigFilePath = configFilePath;
			}

			// If there's no Roadkill section, ConfigFileManager is in an invalid state
			_section = _config.GetSection("roadkill") as RoadkillSection;
			if (_section == null)
			{
				string errorMessage = "";

				if (_isWebConfig)
					errorMessage = "The web.config file does not contain a Roadkill section";
				else
					errorMessage = string.Format("The config file{0} does not contain a Roadkill section", ConfigFilePath);

				throw new InvalidOperationException(errorMessage);
			}
		}

		/// <summary>
		/// Tests the web.config can be saved to by changing the "installed" to false.
		/// </summary>
		/// <returns>Any error messages or an empty string if no errors occurred.</returns>
		public override string TestSaveWebConfig()
		{
			try
			{
				ResetInstalledState();
				return "";
			}
			catch (Exception e)
			{
				return e.ToString();
			}
		}

		public override void UpdateCurrentVersion(string currentVersion)
		{
			try
			{
				RoadkillSection section = _config.GetSection("roadkill") as RoadkillSection;
				section.Version = currentVersion;
				_config.Save(ConfigurationSaveMode.Minimal);
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new UpgradeException("An exception occurred while updating the version to the web.config", ex);
			}
		}

		public override RoadkillSection Load()
		{
			return _section;
		}

		public override void Save(SettingsViewModel settings)
		{
			try
			{
				if (settings.UseWindowsAuth)
					WriteConfigForWindowsAuth();
				else
					WriteConfigForFormsAuth(settings);

				// Create a "Roadkill" connection string, or use the existing one if it exists.
				ConnectionStringSettings roadkillConnection = new ConnectionStringSettings("Roadkill", settings.ConnectionString);

				if (_config.ConnectionStrings.ConnectionStrings["Roadkill"] == null)
					_config.ConnectionStrings.ConnectionStrings.Add(roadkillConnection);
				else
					_config.ConnectionStrings.ConnectionStrings["Roadkill"].ConnectionString = settings.ConnectionString;

				// The roadkill section
				DataStoreType dataStoreType = DataStoreType.ByName(settings.DataStoreTypeName);
				RoadkillSection section = _config.GetSection("roadkill") as RoadkillSection;
				section.AdminRoleName = settings.AdminRoleName;
				section.AttachmentsFolder = settings.AttachmentsFolder;
				section.UseObjectCache = settings.UseObjectCache;
				section.UseBrowserCache = settings.UseBrowserCache;
				section.ConnectionStringName = "Roadkill";
				section.DataStoreType = dataStoreType.Name;
				section.EditorRoleName = settings.EditorRoleName;
				section.LdapConnectionString = settings.LdapConnectionString;
				section.LdapUsername = settings.LdapUsername;
				section.LdapPassword = settings.LdapPassword;
				section.RepositoryType = dataStoreType.CustomRepositoryType;
				section.UseWindowsAuthentication = settings.UseWindowsAuth;
				section.Version = ApplicationSettings.FileVersion.ToString();

				// For first time installs: these need to be explicit as the DefaultValue="" in the attribute doesn't determine the value when saving.
				section.IsPublicSite = settings.IsPublicSite;
				section.IgnoreSearchIndexErrors = settings.IgnoreSearchIndexErrors;	
				section.Installed = true;

				_config.Save(ConfigurationSaveMode.Minimal);
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new InstallerException(ex, "An exception occurred while updating the settings to the web.config");
			}
		}

		public override void UpdateLanguage(string uiLanguageCode)
		{
			try
			{
				GlobalizationSection globalizationSection = _config.GetSection("system.web/globalization") as GlobalizationSection;
				globalizationSection.UICulture = uiLanguageCode;
				_config.Save(ConfigurationSaveMode.Minimal);
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new ConfigurationException("An exception occurred while updating the UI language in the web.config", ex);
			}
		}

		/// <summary>
		/// Adds config settings for forms authentication.
		/// </summary>
		private void WriteConfigForFormsAuth(SettingsViewModel summary)
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
		public override void ResetInstalledState()
		{
			try
			{
				_section.Installed = false;
				_config.Save(ConfigurationSaveMode.Minimal);
			}
			catch (ConfigurationErrorsException ex)
			{
				throw new InstallerException(ex, "An exception occurred while resetting web.config install state to false.");
			}
		}

		/// <summary>
		/// Loads the settings from the configuration file.
		/// </summary>
		/// <param name="config">The configuration to load the settings from. If this is null, the <see cref="ConfigurationManager"/> 
		/// is used to load the settings.</param>
		public override ApplicationSettings GetApplicationSettings()
		{
			ApplicationSettings appSettings = new ApplicationSettings();

			appSettings.AdminRoleName = _section.AdminRoleName;
			appSettings.AttachmentsFolder = _section.AttachmentsFolder;
			appSettings.AttachmentsRoutePath = _section.AttachmentsRoutePath;
			appSettings.ConnectionStringName = _section.ConnectionStringName;
			appSettings.ConnectionString = _config.ConnectionStrings.ConnectionStrings[_section.ConnectionStringName].ConnectionString;
			if (string.IsNullOrEmpty(appSettings.ConnectionString))
				appSettings.ConnectionString = ConfigurationManager.ConnectionStrings[_section.ConnectionStringName].ConnectionString;

			if (string.IsNullOrEmpty(appSettings.ConnectionString))
				Log.Warn("ConnectionString property is null/empty.");

			// Ignore the legacy useCache and cacheText section keys, as the behaviour has changed.
			appSettings.UseObjectCache = _section.UseObjectCache;
			appSettings.UseBrowserCache = _section.UseBrowserCache;

			// Look for the legacy database type key
			string dataStoreType = _section.DataStoreType;
			if (string.IsNullOrEmpty(dataStoreType) && !string.IsNullOrEmpty(_section.DatabaseType))
				dataStoreType = _section.DatabaseType;

			appSettings.LoggingTypes = _section.Logging;
			appSettings.LogErrorsOnly = _section.LogErrorsOnly;
			appSettings.DataStoreType = DataStoreType.ByName(dataStoreType);
			appSettings.ConnectionStringName = _section.ConnectionStringName;
			appSettings.EditorRoleName = _section.EditorRoleName;
			appSettings.IgnoreSearchIndexErrors = _section.IgnoreSearchIndexErrors;
			appSettings.IsPublicSite = _section.IsPublicSite;
			appSettings.Installed = _section.Installed;
			appSettings.LdapConnectionString = _section.LdapConnectionString;
			appSettings.LdapUsername = _section.LdapUsername;
			appSettings.LdapPassword = _section.LdapPassword;
			appSettings.RepositoryType = _section.RepositoryType;
			appSettings.UseHtmlWhiteList = _section.UseHtmlWhiteList;
			appSettings.UserManagerType = _section.UserManagerType;
			appSettings.UseWindowsAuthentication = _section.UseWindowsAuthentication;
			appSettings.UpgradeRequired = UpgradeChecker.IsUpgradeRequired(_section.Version);

			return appSettings;
		}

		public System.Configuration.Configuration GetConfiguration()
		{
			return _config;
		}
	}
}
