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
	public class FullTrustConfigReader : ConfigReader
	{
		private System.Configuration.Configuration _config;
		private bool _isWebConfig;
		private bool _isConfigLoaded;
		private RoadkillSection _section;

		public FullTrustConfigReader(string configFilePath) : base(configFilePath)
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

		public override void Save(SettingsSummary settings)
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

				// Optional "tweak" settings - these need to be explicit as DefaultValue="" in the attribute doesn't determine the value when saving.
				section.IsPublicSite = true;
				section.IgnoreSearchIndexErrors = true;
				section.ResizeImages = true;
				section.Version = ApplicationSettings.ProductVersion.ToString();

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
			ApplicationSettings settings = new ApplicationSettings();

			settings.AdminRoleName = _section.AdminRoleName;
			settings.AttachmentsFolder = _section.AttachmentsFolder;
			settings.AttachmentsRoutePath = _section.AttachmentsRoutePath;
			settings.ConnectionStringName = _section.ConnectionStringName;
			settings.ConnectionString = _config.ConnectionStrings.ConnectionStrings[_section.ConnectionStringName].ConnectionString;
			if (string.IsNullOrEmpty(settings.ConnectionString))
				settings.ConnectionString = ConfigurationManager.ConnectionStrings[_section.ConnectionStringName].ConnectionString;

			if (string.IsNullOrEmpty(settings.ConnectionString))
				Log.Warn("ConnectionString property is null/empty.");

			// Ignore the legacy useCache and cacheText section keys, as the behaviour has changed.
			settings.UseObjectCache = _section.UseObjectCache;
			settings.UseBrowserCache = _section.UseBrowserCache;

			// Look for the legacy database type key
			string dataStoreType = _section.DataStoreType;
			if (string.IsNullOrEmpty(dataStoreType) && !string.IsNullOrEmpty(_section.DatabaseType))
				dataStoreType = _section.DatabaseType;

			settings.LoggingTypes = _section.Logging;
			settings.LogErrorsOnly = _section.LogErrorsOnly;
			settings.DataStoreType = DataStoreType.ByName(dataStoreType);
			settings.ConnectionStringName = _section.ConnectionStringName;
			settings.EditorRoleName = _section.EditorRoleName;
			settings.IgnoreSearchIndexErrors = _section.IgnoreSearchIndexErrors;
			settings.IsPublicSite = _section.IsPublicSite;
			settings.Installed = _section.Installed;
			settings.LdapConnectionString = _section.LdapConnectionString;
			settings.LdapUsername = _section.LdapUsername;
			settings.LdapPassword = _section.LdapPassword;
			settings.RepositoryType = _section.RepositoryType;
			settings.ResizeImages = _section.ResizeImages;
			settings.UseHtmlWhiteList = _section.UseHtmlWhiteList;
			settings.UserManagerType = _section.UserManagerType;
			settings.UseWindowsAuthentication = _section.UseWindowsAuthentication;
			settings.UpgradeRequired = UpgradeChecker.IsUpgradeRequired(_section.Version);

			return settings;
		}

		public System.Configuration.Configuration GetConfiguration()
		{
			return _config;
		}
	}
}
