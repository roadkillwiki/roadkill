using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Roadkill.Core.Configuration
{
	internal class XmlConfigReader
	{
		private string _configFilePath;

		public XmlConfigReader(string configFilePath)
		{
			if (!File.Exists(configFilePath))
				throw new ConfigurationException(null, "The XML config file {0} could not be found", configFilePath);

			_configFilePath = configFilePath;
		}

		public RoadkillSection ReadSection()
		{
			XDocument document = XDocument.Load(_configFilePath);
			XElement element = document.Descendants().FirstOrDefault(x => x.Name == "roadkill");
			if (element == null)
				throw new ConfigurationException(null, "The file {0} does not contain a <roadkill> node", _configFilePath);

			RoadkillSection section = new RoadkillSection();

			// Hardcoding the names is fine, as changing the names will break backward compatibility anyway
			section.AdminRoleName = element.Attribute("adminRoleName").Value;
			section.AttachmentsRoutePath = element.Attribute("attachmentsRoutePath").Value;
			section.AttachmentsFolder = element.Attribute("attachmentsFolder").Value;
			section.UseObjectCache = Convert.ToBoolean(element.Attribute("useObjectCache").Value);
			section.UseBrowserCache = Convert.ToBoolean(element.Attribute("useBrowserCache").Value);
			section.ConnectionStringName = element.Attribute("connectionStringName").Value;
			section.DataStoreType = element.Attribute("dataStoreType").Value;
			section.EditorRoleName = element.Attribute("editorRoleName").Value;
			section.IgnoreSearchIndexErrors = Convert.ToBoolean(element.Attribute("ignoreSearchIndexErrors").Value);
			section.Installed = Convert.ToBoolean(element.Attribute("installed").Value);
			section.IsPublicSite = Convert.ToBoolean(element.Attribute("isPublicSite").Value);
			section.LdapConnectionString = element.Attribute("ldapConnectionString").Value;
			section.LdapUsername = element.Attribute("ldapUsername").Value;
			section.LdapPassword = element.Attribute("ldapPassword").Value;
			section.Logging = element.Attribute("logging").Value;
			section.LogErrorsOnly = Convert.ToBoolean(element.Attribute("logErrorsOnly").Value);
			section.RepositoryType = element.Attribute("repositoryType").Value;
			section.ResizeImages = Convert.ToBoolean(element.Attribute("resizeImages").Value);
			section.UserManagerType = element.Attribute("userManagerType").Value;
			section.UseHtmlWhiteList = Convert.ToBoolean(element.Attribute("useHtmlWhiteList").Value);
			section.UseWindowsAuthentication = Convert.ToBoolean(element.Attribute("useWindowsAuthentication").Value);
			section.Version = element.Attribute("version").Value;
			
			return section;
		}
	}
}
