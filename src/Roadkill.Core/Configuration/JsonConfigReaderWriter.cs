using Roadkill.Core.Database;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Reads and writes the application configuration settings from a JSON file (roadkill.json by default).
	/// </summary>
	public class JsonConfigReaderWriter : ConfigReaderWriter
	{
		/// <summary>
		/// The default filename for the Roadkill settings, found in the application's content root.
		/// </summary>
		public static readonly string DefaultFilename = "roadkill.json";

		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions()
		{
			WriteIndented = true,
			PropertyNameCaseInsensitive = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true
		};

		private static readonly object _fileLock = new object();
		private RoadkillSection _section;

		/// <summary>
		/// The full path to the JSON settings file.
		/// </summary>
		public string ConfigFilePath { get; private set; }

		/// <summary>
		/// The content root of the application, used to resolve "~/" paths.
		/// </summary>
		public string ContentRootPath { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JsonConfigReaderWriter"/> class. If the file doesn't exist,
		/// a new uninstalled configuration is created (but not saved until <see cref="Save"/> is called).
		/// </summary>
		/// <param name="configFilePath">The full path to the JSON file.</param>
		/// <param name="contentRootPath">The content root of the site, used to resolve "~/" paths. If empty, the directory of the config file is used.</param>
		public JsonConfigReaderWriter(string configFilePath, string contentRootPath = "")
		{
			if (string.IsNullOrEmpty(configFilePath))
				throw new ArgumentNullException(nameof(configFilePath));

			ConfigFilePath = configFilePath;
			ContentRootPath = string.IsNullOrEmpty(contentRootPath) ? Path.GetDirectoryName(Path.GetFullPath(configFilePath)) : contentRootPath;
			_section = ReadFile();
		}

		private RoadkillSection ReadFile()
		{
			lock (_fileLock)
			{
				if (!File.Exists(ConfigFilePath))
					return new RoadkillSection();

				try
				{
					string json = File.ReadAllText(ConfigFilePath);
					if (string.IsNullOrWhiteSpace(json))
						return new RoadkillSection();

					return JsonSerializer.Deserialize<RoadkillSection>(json, _jsonOptions) ?? new RoadkillSection();
				}
				catch (JsonException ex)
				{
					throw new ConfigurationException(ex, "The config file {0} is not valid JSON: {1}", ConfigFilePath, ex.Message);
				}
			}
		}

		private void WriteFile()
		{
			lock (_fileLock)
			{
				string directory = Path.GetDirectoryName(ConfigFilePath);
				if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
					Directory.CreateDirectory(directory);

				string json = JsonSerializer.Serialize(_section, _jsonOptions);
				File.WriteAllText(ConfigFilePath, json);
			}
		}

		/// <summary>
		/// Updates the current UI language and saves the configuration file.
		/// </summary>
		/// <param name="uiLanguageCode">The UI language code, e.g. fr for French.</param>
		/// <exception cref="ConfigurationException">An exception occurred while updating the UI language.</exception>
		public override void UpdateLanguage(string uiLanguageCode)
		{
			try
			{
				_section.UiLanguage = uiLanguageCode;
				WriteFile();
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				throw new ConfigurationException("An exception occurred while updating the UI language in the config file", ex);
			}
		}

		/// <summary>
		/// Loads the Roadkill-specific configuration settings.
		/// </summary>
		public override RoadkillSection Load()
		{
			return _section;
		}

		/// <summary>
		/// Saves the configuration settings. This will save a subset of the <see cref="SettingsViewModel" /> based on
		/// the values that match those found in the <see cref="RoadkillSection" />
		/// </summary>
		/// <exception cref="InstallerException">An exception occurred while updating the settings.</exception>
		public override void Save(SettingsViewModel settings)
		{
			try
			{
				_section.AdminRoleName = settings.AdminRoleName;
				_section.AttachmentsFolder = settings.AttachmentsFolder;
				_section.AzureConnectionString = settings.AzureConnectionString;
				_section.AzureContainer = settings.AzureContainer;
				_section.UseAzureFileStorage = settings.UseAzureFileStorage;
				_section.UseObjectCache = settings.UseObjectCache;
				_section.UseBrowserCache = settings.UseBrowserCache;
				_section.ConnectionString = settings.ConnectionString;
				_section.DatabaseName = string.IsNullOrEmpty(settings.DatabaseName) ? SupportedDatabases.SqlServer2008.Id : settings.DatabaseName;
				_section.EditorRoleName = settings.EditorRoleName;
				_section.IsPublicSite = settings.IsPublicSite;
				_section.IgnoreSearchIndexErrors = settings.IgnoreSearchIndexErrors;
				_section.Installed = true;

				WriteFile();
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				throw new InstallerException(ex, "An exception occurred while updating the settings to the config file");
			}
		}

		/// <summary>
		/// Resets the state the configuration file/store so the 'installed' property is false.
		/// </summary>
		/// <exception cref="InstallerException">An exception occurred while resetting the install state to false.</exception>
		public override void ResetInstalledState()
		{
			try
			{
				_section.Installed = false;
				WriteFile();
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				throw new InstallerException(ex, "An exception occurred while resetting the config file install state to false.");
			}
		}

		/// <summary>
		/// Gets the current application settings.
		/// </summary>
		public override ApplicationSettings GetApplicationSettings()
		{
			ApplicationSettings appSettings = new ApplicationSettings(ContentRootPath);

			appSettings.AdminRoleName = _section.AdminRoleName;
			appSettings.AttachmentsFolder = _section.AttachmentsFolder;
			appSettings.AttachmentsRoutePath = string.IsNullOrEmpty(_section.AttachmentsRoutePath) ? "Attachments" : _section.AttachmentsRoutePath;
			appSettings.ApiKeys = ParseApiKeys(_section.ApiKeys);
			appSettings.AzureConnectionString = _section.AzureConnectionString;
			appSettings.AzureContainer = _section.AzureContainer;
			appSettings.ConnectionString = _section.ConnectionString;
			appSettings.UseObjectCache = _section.UseObjectCache;
			appSettings.UseBrowserCache = _section.UseBrowserCache;
			appSettings.DatabaseName = string.IsNullOrEmpty(_section.DatabaseName) ? SupportedDatabases.SqlServer2008.Id : _section.DatabaseName;
			appSettings.EditorRoleName = _section.EditorRoleName;
			appSettings.IgnoreSearchIndexErrors = _section.IgnoreSearchIndexErrors;
			appSettings.IsPublicSite = _section.IsPublicSite;
			appSettings.Installed = _section.Installed;
			appSettings.IsDemoSite = _section.IsDemoSite;
			appSettings.UseAzureFileStorage = _section.UseAzureFileStorage;
			appSettings.UseHtmlWhiteList = _section.UseHtmlWhiteList;
			appSettings.UserServiceType = _section.UserServiceType;
			appSettings.UiLanguage = string.IsNullOrEmpty(_section.UiLanguage) ? "en" : _section.UiLanguage;
			appSettings.Smtp = new SmtpSettings()
			{
				Host = _section.SmtpHost,
				Port = _section.SmtpPort,
				Username = _section.SmtpUsername,
				Password = _section.SmtpPassword,
				EnableSsl = _section.SmtpEnableSsl,
				From = _section.SmtpFrom,
				PickupDirectory = _section.SmtpPickupDirectory
			};

			return appSettings;
		}

		private IEnumerable<string> ParseApiKeys(string apiKeys)
		{
			var keyList = new List<string>();
			if (string.IsNullOrEmpty(apiKeys))
				return keyList;

			foreach (string item in apiKeys.Split(','))
			{
				if (!string.IsNullOrWhiteSpace(item))
					keyList.Add(item.Trim());
			}

			return keyList;
		}

		/// <summary>
		/// Tests the config file to ensure that it can be written to.
		/// </summary>
		/// <returns>An empty string if no error occurred; otherwise the error message.</returns>
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
