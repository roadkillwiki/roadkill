using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Xml.Linq;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Defines a class responsible for reading and writing application settings/configuration.
	/// </summary>
	public abstract class ConfigReaderWriter
	{
		/// <summary>
		/// Updates the current version in the RoadkillSection and saves the configuration file.
		/// </summary>
		/// <param name="currentVersion">The current version.</param>
		public abstract void UpdateCurrentVersion(string currentVersion);

		/// <summary>
		/// Updates the current UI language in the globalization section and saves the configuration file.
		/// </summary>
		/// <param name="uiLanguageCode">The UI language code, e.g. fr for French.</param>
		/// <exception cref="ConfigurationException">An exception occurred while updating the UI language.</exception>
		public abstract void UpdateLanguage(string uiLanguageCode);

		/// <summary>
		/// Saves the configuration settings. This will save a subset of the <see cref="SettingsViewModel"/> based on 
		/// the values that match those found in the <see cref="RoadkillSection"/>
		/// </summary>
		/// <param name="settings">The application settings.</param>
		/// <exception cref="InstallerException">An exception occurred while updating the settings.</exception>
		public abstract void Save(SettingsViewModel settings);

		/// <summary>
		/// Loads the Roadkill-specific configuration settings.
		/// </summary>
		/// <returns>A <see cref="RoadkillSection"/> instance with the settings.</returns>
		public abstract RoadkillSection Load();

		/// <summary>
		/// Gets the current application settings, which is usually cached settings from the <see cref="Load"/> method.
		/// </summary>
		/// <returns>A new <see cref="ApplicationSettings"/> instance</returns>
		public abstract ApplicationSettings GetApplicationSettings();

		/// <summary>
		/// Resets the state the configuration file/store so the 'installed' property is false.
		/// </summary>
		/// <exception cref="InstallerException">An web.config related error occurred while reseting the install state.</exception>
		public abstract void ResetInstalledState();

		/// <summary>
		/// Tests the app.config or web.config file to ensure that it can be written to.
		/// </summary>
		/// <returns>An empty string if no error occurred; otherwise the error message.</returns>
		public abstract string TestSaveWebConfig();
	}
}
