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

namespace Roadkill.Core
{
	public class ApplicationSettings
	{
		public RoadkillSection section { get; set; }
		public string AdminRoleName { get; set; }
		public string AppDataPath { get; set; }	
		public string AttachmentsFolder { get; set; }
		public string AttachmentsUrlPath { get; set; }
		public string AttachmentsRoutePath { get; set; }
		public bool CachedEnabled { get; set; }
		public bool CacheText { get; set; }
		public string ConnectionString { get; set; }
		public string ConnectionStringName { get; set; }
		public string CustomTokensPath { get; set; }
		public DatabaseType DatabaseType { get; set; }
		public string EditorRoleName { get; set; }
		public bool IgnoreSearchIndexErrors { get; set; }
		public bool IsPublicSite { get; set; }
		public bool Installed { get; set; }
		public string LdapConnectionString { get; set; }
		public string LdapUsername { get; set; }
		public string LdapPassword { get; set; }
		public int MinimumPasswordLength { get; set; }
		public bool ResizeImages { get; set; }
		public string UserManagerType { get; set; }
		public bool UseWindowsAuthentication { get; set; }
		public string Version { get; set; }

		public virtual void Load(RoadkillSection section)
		{
			AdminRoleName = section.AdminRoleName;		
			AppDataPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\";

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
			CachedEnabled = section.CacheEnabled;
			CacheText = section.CacheText;
			ConnectionString = ConfigurationManager.ConnectionStrings[section.ConnectionStringName].ConnectionString;
			ConnectionStringName = section.ConnectionStringName;
			CustomTokensPath = Path.Combine(AppDataPath, "tokens.xml");

			DatabaseType dbType;
			if (Enum.TryParse<DatabaseType>(section.DatabaseType, true, out dbType))
				DatabaseType = dbType;
			else
				DatabaseType = DatabaseType.SqlServer2005;

			EditorRoleName = section.EditorRoleName;
			IgnoreSearchIndexErrors = section.IgnoreSearchIndexErrors;
			IsPublicSite = section.IsPublicSite;
			Installed = section.Installed;		
			LdapConnectionString = section.LdapConnectionString;
			LdapUsername = section.LdapUsername;
			LdapPassword = section.LdapPassword;
			MinimumPasswordLength = 6;
			ResizeImages = section.ResizeImages;
			UserManagerType = section.UserManagerType;
			UseWindowsAuthentication = section.UseWindowsAuthentication;
			Version = typeof(RoadkillSettings).Assembly.GetName().Version.ToString();
		}

		/// <summary>
		/// Loads a custom app.config file for the settings, overriding the default application config file.
		/// </summary>
		/// <param name="filePath">A full path to the config file.</param>
		public void LoadCustomConfigFile(string filePath)
		{
			ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
			fileMap.ExeConfigFilename = filePath;
			Configuration cfg = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

			Load(cfg.GetSection("roadkill") as RoadkillSection);
		}
	}
}