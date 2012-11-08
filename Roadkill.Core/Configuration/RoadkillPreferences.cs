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
	/// <summary>
	/// 
	/// </summary>
	public class RoadkillPreferences
	{
		public RoadkillSection ConfigurationFileSettings { get; set; }
		public SiteConfiguration DatabaseStoreSettings { get; set; }
		public bool AllowUserSignup { get; set; }
		public string AdminRoleName { get; set; }
		public IList<string> AllowedFileTypes { get; set; }
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
		public bool IsRecaptchaEnabled { get; set; }
		public string LdapConnectionString { get; set; }
		public string LdapUsername { get; set; }
		public string LdapPassword { get; set; }
		public string MarkupType { get; set; }
		public int MinimumPasswordLength { get; set; }
		public string RecaptchaPrivateKey { get; set; }
		public string RecaptchaPublicKey { get; set; }
		public bool ResizeImages { get; set; }
		public string SiteName { get; set; }
		public string SiteUrl { get; set; }
		public string Theme { get; set; }
		public string Title { get; set; }
		public string ThemePath { get; set; }
		public string UserManagerType { get; set; }
		public bool UseWindowsAuthentication { get; set; }
		public string Version { get; set; }

		public static IConfigurationContainer Current
		{
			get
			{
				return ObjectFactory.GetInstance<IConfigurationContainer>();
			}
		}

		public void Refresh()
		{
			IRepository repository = ObjectFactory.GetInstance<IRepository>();

			ConfigurationFileSettings = ConfigurationManager.GetSection("roadkill") as RoadkillSection;
			DatabaseStoreSettings = repository.Queryable<SiteConfiguration>().FirstOrDefault(s => s.Id == SiteConfiguration.ConfigurationId);

			if (DatabaseStoreSettings == null)
				throw new DatabaseException(null, "No configuration settings could be found in the database (id {0}). " +
					"Has SettingsManager.SaveSiteConfiguration() been called?", SiteConfiguration.ConfigurationId);

			if (string.IsNullOrEmpty(DatabaseStoreSettings.AllowedFileTypes))
				throw new InvalidOperationException("The allowed file types setting is empty");

			AllowUserSignup = DatabaseStoreSettings.AllowUserSignup;
			AdminRoleName = ConfigurationFileSettings.AdminRoleName;
			AllowedFileTypes = new List<string>(DatabaseStoreSettings.AllowedFileTypes.Replace(" ", "").Split(','));
			AppDataPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\";

			if (ConfigurationFileSettings.AttachmentsFolder.StartsWith("~") && HttpContext.Current != null)
			{
				AttachmentsFolder = HttpContext.Current.Server.MapPath(ConfigurationFileSettings.AttachmentsFolder);
			}
			else
			{
				AttachmentsFolder = ConfigurationFileSettings.AttachmentsFolder;
			}

			AttachmentsUrlPath = "/Attachments";
			AttachmentsRoutePath = "Attachments";
			CachedEnabled = ConfigurationFileSettings.CacheEnabled;
			CacheText = ConfigurationFileSettings.CacheText;
			ConnectionString = ConfigurationManager.ConnectionStrings[ConfigurationFileSettings.ConnectionStringName].ConnectionString;
			ConnectionStringName = ConfigurationFileSettings.ConnectionStringName;
			CustomTokensPath = Path.Combine(AppDataPath, "tokens.xml");

			DatabaseType dbType;
			if (Enum.TryParse<DatabaseType>(ConfigurationFileSettings.DatabaseType, true, out dbType))
				DatabaseType = dbType;
			else
				DatabaseType = DatabaseType.SqlServer2005;

			EditorRoleName = ConfigurationFileSettings.EditorRoleName;
			IgnoreSearchIndexErrors = ConfigurationFileSettings.IgnoreSearchIndexErrors;
			IsPublicSite = ConfigurationFileSettings.IsPublicSite;
			Installed = ConfigurationFileSettings.Installed;
			IsRecaptchaEnabled = DatabaseStoreSettings.EnableRecaptcha;
			LdapConnectionString = ConfigurationFileSettings.LdapConnectionString;
			LdapUsername = ConfigurationFileSettings.LdapUsername;
			LdapPassword = ConfigurationFileSettings.LdapPassword;
			MarkupType = DatabaseStoreSettings.MarkupType;
			MinimumPasswordLength = 6;
			RecaptchaPrivateKey = DatabaseStoreSettings.RecaptchaPrivateKey;
			RecaptchaPublicKey = DatabaseStoreSettings.RecaptchaPublicKey;
			ResizeImages = ConfigurationFileSettings.ResizeImages;
			SiteName = DatabaseStoreSettings.Title;
			SiteUrl = DatabaseStoreSettings.SiteUrl;
			Theme = DatabaseStoreSettings.Theme;
			Title = DatabaseStoreSettings.Title;
			ThemePath = string.Format("~/Themes/{0}", Theme);
			UserManagerType = ConfigurationFileSettings.UserManagerType;
			UseWindowsAuthentication = ConfigurationFileSettings.UseWindowsAuthentication;
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

			ConfigurationFileSettings = cfg.GetSection("roadkill") as RoadkillSection;
		}
	}
}