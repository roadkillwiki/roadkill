using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.Web.Configuration;
using System.Reflection;
using System.IO;
using StructureMap;
using Roadkill.Core.Database;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Contains all settings that require an application (appdomain) restart when changed - typically stored in a .config file.
	/// </summary>
	public class ApplicationSettings
	{
		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create,edit,delete pages,
		/// manage users, manage site settings and use the admin tools.
		/// </summary>
		public string AdminRoleName { get; set; }

		/// <summary>
		/// The path to the App data folder.
		/// </summary>
		public string AppDataPath { get; set; }

		/// <summary>
		/// The folder where all uploads (typically image files) are saved to. This is taken from the web.config,
		/// if the setting uses "~/" then the path is translated into one that is relative to the site root, 
		/// otherwise it is assumed to be an absolute file path.
		/// </summary>
		public string AttachmentsFolder { get; set; }

		/// <summary>
		/// The route used for all attachment HTTP requests (currently non-user configurable). Should not contain a trailing slash.
		/// </summary>
		public string AttachmentsUrlPath { get; set; }

		/// <summary>
		/// The route used for all attachment HTTP requests (currently non-user configurable), minus any starting "/".
		/// </summary>
		public string AttachmentsRoutePath { get; set; }

		/// <summary>
		/// The connection string to the Roadkill database.
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// The connection string name (held in the connection strings section of the config file) for the Roadkill database.
		/// </summary>
		public string ConnectionStringName { get; set; }

		/// <summary>
		/// The file path for the custom tokens file.
		/// </summary>
		public string CustomTokensPath { get; set; }

		/// <summary>
		/// The database type used as the backing store.
		/// </summary>
		public DataStoreType DataStoreType { get; set; }

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create and edit pages.
		/// </summary>
		public string EditorRoleName { get; set; }

		/// <summary>
		/// The file path for the html element white list file.
		/// </summary>
		public string HtmlElementWhiteListPath { get; set; }

		/// <summary>
		/// Whether errors in updating the lucene index throw exceptions or are just ignored.
		/// </summary>
		public bool IgnoreSearchIndexErrors { get; set; }

		/// <summary>
		/// Whether the site is public, i.e. all pages are visible by default. This is optional in the web.config and the default is true.
		/// </summary>
		public bool IsPublicSite { get; set; }

		/// <summary>
		/// Indicates whether the installation has been completed previously.
		/// </summary>
		public bool Installed { get; set; }

		/// <summary>
		/// The connection string for Active Directory server if <see cref="UseWindowsAuthentication"/> is true.
		/// This should start with LDAP:// in uppercase.
		/// </summary>
		public string LdapConnectionString { get; set; }

		/// <summary>
		/// The username to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		public string LdapUsername { get; set; }

		/// <summary>
		/// The password to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		public string LdapPassword { get; set; }

		/// <summary>
		/// The number of characters each password should be.
		/// </summary>
		public int MinimumPasswordLength { get; set; }

		/// <summary>
		/// The fully qualified assembly and classname for the repository.
		/// </summary>
		public string RepositoryType { get; set; }

		/// <summary>
		/// Whether to scale images dynamically on the page, using Javascript, so they fit inside the main page container (400x400px).
		/// </summary>
		public bool ResizeImages { get; set; }

		/// <summary>
		/// True if the version number in the web.config does not match the current assembly version.
		/// </summary>
		public bool UpgradeRequired { get; internal set; }

		/// <summary>
		/// Indicates whether server-based page object caching is enabled.
		/// </summary>
		public bool UseObjectCache { get; set; }

		/// <summary>
		/// Indicates whether to send HTTP cache headers to the browser (304 not modified)
		/// </summary>
		public bool UseBrowserCache { get; set; }

		/// <summary>
		/// Gets a value indicating whether the html that is converted from the markup is 
		/// cleaned for tags, using the App_Data/htmlwhitelist.xml file.
		/// </summary>
		public bool UseHtmlWhiteList { get; set; }

		/// <summary>
		/// The type for the <see cref="UserManager"/>. If the setting for this is blank
		/// in the web.config, then the <see cref="UseWindowsAuthentication"/> is checked and if false
		/// a <see cref="DefaultUserManager"/> is created. The format of this setting can be retrieved by
		/// using <code>typeof(YourUserManager).AssemblyQualifiedName.</code>
		/// </summary>
		public string UserManagerType { get; set; }

		/// <summary>
		/// Gets a value indicating whether this windows authentication is being used.
		/// </summary>
		public bool UseWindowsAuthentication { get; set; }

		/// <summary>
		/// The current Roadkill assembly version.
		/// </summary>
		public static Version AssemblyVersion
		{
			get
			{
				return typeof(RoadkillSettings).Assembly.GetName().Version;
			}
		}

		/// <summary>
		/// Loads the settings from the configuration file.
		/// </summary>
		/// <param name="config">The configuration to load the settings from. If this is null, the <see cref="ConfigurationManager"/> 
		/// is used to load the settings.</param>
		public virtual void Load(System.Configuration.Configuration config = null)
		{
			// Configuration options that aren't configured from web.config
			AppDataPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\";
			CustomTokensPath = Path.Combine(AppDataPath, "tokens.xml");
			HtmlElementWhiteListPath = Path.Combine(AppDataPath, "htmlwhitelist.xml");
			MinimumPasswordLength = 6;

			// Web/app.config settings
			RoadkillSection section;

			if (config == null)
			{
				section = ConfigurationManager.GetSection("roadkill") as RoadkillSection;
			}
			else
			{
				section = config.GetSection("roadkill") as RoadkillSection;
			}

			AdminRoleName = section.AdminRoleName;		
			

			if (section.AttachmentsFolder.StartsWith("~") && HttpContext.Current != null)
			{
				AttachmentsFolder = HttpContext.Current.Server.MapPath(section.AttachmentsFolder);
			}
			else
			{
				AttachmentsFolder = section.AttachmentsFolder;
			}

			AttachmentsUrlPath = "/Attachments";
			AttachmentsRoutePath = "Attachments";
			ConnectionStringName = section.ConnectionStringName;

			if (config == null)
			{
				ConnectionString = ConfigurationManager.ConnectionStrings[section.ConnectionStringName].ConnectionString;
			}
			else
			{
				ConnectionString = config.ConnectionStrings.ConnectionStrings[section.ConnectionStringName].ConnectionString;
			}

			if (string.IsNullOrEmpty(ConnectionString))
				Log.Warn("ConnectionString property is null/empty.");

			// Ignore the legacy useCache and cacheText section keys, as the behaviour has changed.
			UseObjectCache = section.UseObjectCache;
			UseBrowserCache = section.UseBrowserCache;

			// Look for the legacy database type key
			string dataStoreType = section.DataStoreType;
			if (string.IsNullOrEmpty(dataStoreType) && !string.IsNullOrEmpty(section.DatabaseType))
				dataStoreType = section.DatabaseType;

			DataStoreType = DataStoreType.ByName(dataStoreType);
			ConnectionStringName = section.ConnectionStringName;
			EditorRoleName = section.EditorRoleName;
			IgnoreSearchIndexErrors = section.IgnoreSearchIndexErrors;
			IsPublicSite = section.IsPublicSite;
			Installed = section.Installed;		
			LdapConnectionString = section.LdapConnectionString;
			LdapUsername = section.LdapUsername;
			LdapPassword = section.LdapPassword;
			RepositoryType = section.RepositoryType;
			ResizeImages = section.ResizeImages;
			UseHtmlWhiteList = section.UseHtmlWhiteList;
			UserManagerType = section.UserManagerType;
			UseWindowsAuthentication = section.UseWindowsAuthentication;

			if (string.IsNullOrEmpty(section.Version))
			{
				UpgradeRequired = true;
			}
			else
			{
				Version configVersion = null;
				if (Version.TryParse(section.Version, out configVersion))
				{
					UpgradeRequired = (configVersion != AssemblyVersion);
				}
				else
				{
					Log.Warn("Invalid Version found ({0}) in the web.config, assuming it's the same as the assembly version ({1})", section.Version, AssemblyVersion);
					UpgradeRequired = false;
				}
			}
		}

		/// <summary>
		/// Loads a custom app.config file for the settings, overriding the default application config file.
		/// </summary>
		/// <param name="filePath">A full path to the config file.</param>
		public void LoadCustomConfigFile(string filePath)
		{
			ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
			fileMap.ExeConfigFilename = filePath;
			System.Configuration.Configuration cfg = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

			Load(cfg);
		}
	}
}