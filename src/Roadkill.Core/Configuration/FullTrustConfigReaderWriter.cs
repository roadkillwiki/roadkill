using Roadkill.Core.Database;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.ViewModels;
using System;
using System.Configuration;
using System.IO;
using System.Web.Configuration;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Reads and write the application configuration settings, from a web.config or app.config file.
	/// </summary>
	public class FullTrustConfigReaderWriter : ConfigReaderWriter
	{
		private System.Configuration.Configuration _config;
		private bool _isWebConfig;
		private bool _isConfigLoaded;
		private RoadkillSection _section;

		public string ConfigFilePath { get;set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FullTrustConfigReaderWriter"/> class.
		/// The class will attempt to load the Roadkill section from the web.config when the class 
		/// is instantiated, and cache it for the lifetime of the <see cref="FullTrustConfigReaderWriter"/> instance.
		/// </summary>
		public FullTrustConfigReaderWriter()
			: this("")
		{
		}

		internal FullTrustConfigReaderWriter(string configFilePath)
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
		/// Updates the current version in the RoadkillSection and saves the configuration file.
		/// </summary>
		/// <param name="currentVersion">The current version.</param>
		/// <exception cref="UpgradeException">An exception occurred while updating the version to the web.config</exception>
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

		/// <summary>
		/// Updates the current UI language in the globalization section and saves the configuration file.
		/// </summary>
		/// <param name="uiLanguageCode">The UI language code, e.g. fr for French.</param>
		/// <exception cref="System.Configuration.ConfigurationException">An exception occurred while updating the UI language in the web.config</exception>
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
		/// Loads the Roadkill-specific configuration settings.
		/// </summary>
		/// <returns>
		/// A <see cref="RoadkillSection" /> instance with the settings.
		/// </returns>
		public override RoadkillSection Load()
		{
			return _section;
		}

		/// <summary>
		/// Saves the configuration settings. This will save a subset of the <see cref="SettingsViewModel" /> based on
		/// the values that match those found in the <see cref="RoadkillSection" />
		/// </summary>
		/// <param name="settings">The application settings.</param>
		/// <exception cref="InstallerException">An exception occurred while updating the settings to the web.config</exception>
		public override void Save(SettingsViewModel settings)
		{
			try
			{
				if (settings.UseWindowsAuth)
					WriteConfigForWindowsAuth();
				else
					WriteConfigForFormsAuth();

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
				section.AzureContainer = settings.AzureContainer;
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

		/// <summary>
		/// Adds config settings for forms authentication.
		/// </summary>
		internal void WriteConfigForFormsAuth()
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
		internal void WriteConfigForWindowsAuth()
		{
			// Turn on Windows authentication
			AuthenticationSection authSection = _config.GetSection("system.web/authentication") as AuthenticationSection;
			authSection.Mode = AuthenticationMode.Windows;

			// Turn off anonymous auth
			AnonymousIdentificationSection anonSection = _config.GetSection("system.web/anonymousIdentification") as AnonymousIdentificationSection;
			anonSection.Enabled = false;
		}

		/// <summary>
		/// Resets the state the configuration file/store so the 'installed' property is false.
		/// </summary>
		/// <exception cref="InstallerException">An exception occurred while resetting web.config install state to false.</exception>
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
		/// Gets the current application settings, which is usually cached settings from the <see cref="Load" /> method.
		/// </summary>
		/// <returns>
		/// A new <see cref="ApplicationSettings" /> instance
		/// </returns>
		public override ApplicationSettings GetApplicationSettings()
		{
			ApplicationSettings appSettings = new ApplicationSettings();

			appSettings.AdminRoleName = _section.AdminRoleName;
			appSettings.AttachmentsFolder = _section.AttachmentsFolder;
			appSettings.AttachmentsRoutePath = _section.AttachmentsRoutePath;
			appSettings.AzureConnectionString = _section.AzureConnectionString;
			appSettings.AzureContainer = _section.AzureContainer;

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
			appSettings.UseAzureFileStorage = _section.UseAzureFileStorage;
			appSettings.UseHtmlWhiteList = _section.UseHtmlWhiteList;
			appSettings.UserServiceType = _section.UserServiceType;
			appSettings.UseWindowsAuthentication = _section.UseWindowsAuthentication;
			appSettings.UpgradeRequired = UpgradeChecker.IsUpgradeRequired(_section.Version);

			return appSettings;
		}

		/// <summary>
		/// Gets the curent <see cref="System.Configuration.Configuration"/>.
		/// </summary>
		/// <returns></returns>
		public System.Configuration.Configuration GetConfiguration()
		{
			return _config;
		}

		/// <summary>
		/// Tests the app.config or web.config file to ensure that it can be written to.
		/// </summary>
		/// <returns>
		/// An empty string if no error occurred; otherwise the error message.
		/// </returns>
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
	}
}
