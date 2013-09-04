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

		public override void Save(SettingsSummary settings)
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
			ApplicationSettings settings = new ApplicationSettings();

			if (_section == null)
				_section = Load();

			settings.AdminRoleName = _section.AdminRoleName;
			settings.AttachmentsFolder = _section.AttachmentsFolder;
			settings.AttachmentsRoutePath = _section.AttachmentsRoutePath;
			settings.ConnectionStringName = _section.ConnectionStringName;
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

		public override string TestSaveWebConfig()
		{
			throw new NotImplementedException();
		}
	}
}
