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
	internal class MediumTrustConfigReader : ConfigReader
	{
		private RoadkillSection _section;

		public MediumTrustConfigReader(string configFilePath)
			: base(configFilePath)
		{
			if (string.IsNullOrEmpty(configFilePath))
				throw new ArgumentNullException("configFilePath");

			if (!File.Exists(configFilePath))
				throw new ConfigurationException(null, "The XML config file {0} does not exist on disk.", configFilePath);
		}

		public override void UpdateCurrentVersion(string currentVersion)
		{
			throw new NotImplementedException();
		}

		public override void Save(SettingsViewModel settings)
		{
			throw new NotImplementedException();
		}

		public override void UpdateLanguage(string uiLanguageCode)
		{
			throw new NotImplementedException();
		}

		public override RoadkillSection Load()
		{
			XDocument document = XDocument.Load(ConfigFilePath);
			XElement element = document.Descendants().FirstOrDefault(x => x.Name == "roadkill");
			if (element == null)
				throw new ConfigurationException(null, "The file {0} does not contain a <roadkill> node", ConfigFilePath);

			RoadkillSection section = new RoadkillSection();

			// Hardcoding the names is fine, as changing the names will break backward compatibility anyway
			section.AdminRoleName = element.Attribute("adminRoleName").Value;
			
			section.AttachmentsFolder = element.Attribute("attachmentsFolder").Value;
			section.UseObjectCache = Convert.ToBoolean(element.Attribute("useObjectCache").Value);
			section.UseBrowserCache = Convert.ToBoolean(element.Attribute("useBrowserCache").Value);
			section.ConnectionStringName = element.Attribute("connectionStringName").Value;
			section.EditorRoleName = element.Attribute("editorRoleName").Value;
			section.Installed = Convert.ToBoolean(element.Attribute("installed").Value);
			section.UseWindowsAuthentication = Convert.ToBoolean(element.Attribute("useWindowsAuthentication").Value);
			section.Version = element.Attribute("version").Value;

			//
			// Optional attributes
			//
			if (element.Attribute("attachmentsRoutePath") != null)
				section.AttachmentsRoutePath = element.Attribute("attachmentsRoutePath").Value;

			if (element.Attribute("dataStoreType") != null)
				section.DataStoreType = element.Attribute("dataStoreType").Value;

			if (element.Attribute("ignoreSearchIndexErrors") != null)
				section.IgnoreSearchIndexErrors = Convert.ToBoolean(element.Attribute("ignoreSearchIndexErrors").Value);

			if (element.Attribute("isPublicSite") != null)
				section.IsPublicSite = Convert.ToBoolean(element.Attribute("isPublicSite").Value);

			if (element.Attribute("ldapConnectionString") != null)
				section.LdapConnectionString = element.Attribute("ldapConnectionString").Value;

			if (element.Attribute("ldapUsername") != null)
				section.LdapUsername = element.Attribute("ldapUsername").Value;

			if (element.Attribute("ldapPassword") != null)
				section.LdapPassword = element.Attribute("ldapPassword").Value;

			if (element.Attribute("logging") != null)
				section.Logging = element.Attribute("logging").Value;

			if (element.Attribute("logErrorsOnly") != null)
				section.LogErrorsOnly = Convert.ToBoolean(element.Attribute("logErrorsOnly").Value);

			if (element.Attribute("resizeImages") != null)
				section.ResizeImages = Convert.ToBoolean(element.Attribute("resizeImages").Value);

			if (element.Attribute("useHtmlWhiteList") != null)
				section.UseHtmlWhiteList = Convert.ToBoolean(element.Attribute("useHtmlWhiteList").Value);

			if (element.Attribute("userManagerType") != null)
				section.UserManagerType = element.Attribute("userManagerType").Value;

			if (element.Attribute("repositoryType") != null)
				section.RepositoryType = element.Attribute("repositoryType").Value;
			
			return section;
		}

		public override void ResetInstalledState()
		{
			throw new NotImplementedException();
		}

		public override ApplicationSettings GetApplicationSettings()
		{
			ApplicationSettings appSettings = new ApplicationSettings();

			if (_section == null)
				_section = Load();

			appSettings.AdminRoleName = _section.AdminRoleName;
			appSettings.AttachmentsFolder = _section.AttachmentsFolder;
			appSettings.AttachmentsRoutePath = _section.AttachmentsRoutePath;
			appSettings.ConnectionStringName = _section.ConnectionStringName;
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

		public override string TestSaveWebConfig()
		{
			throw new NotImplementedException();
		}
	}
}
