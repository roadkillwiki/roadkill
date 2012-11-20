using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Represents settings for the site, some of which are stored in the web.config.
	/// </summary>
	[Serializable]
	public class SettingsSummary
	{
		private static string _themesRoot;

		public string AdminEmail { get; set; }
		public string AdminPassword { get; set; }
		public string AdminRoleName { get; set; }
		public string AllowedExtensions { get; set; }
		public bool AllowUserSignup { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_AttachmentsEmpty")]
		public string AttachmentsFolder { get; set; }

		public bool CacheEnabled { get; set; }
		public bool CacheText { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_ConnectionEmpty")]
		public string ConnectionString { get; set; }

		/// <summary>
		/// Used in the intial configuration/installation alongside Fluent CFG.
		/// </summary>
		public DatabaseType DatabaseType { get; set; }
		public IEnumerable<string> DatabaseTypesAvailable
		{
			get
			{
				foreach (DatabaseType dbType in Enum.GetValues(typeof(DatabaseType)))
				{
					yield return dbType.ToString();
				}
			}
		}

		public string EditorRoleName { get; set; }
		public bool EnableRecaptcha { get; set; }
		
		public string LdapConnectionString { get; set; }
		public string LdapUsername { get; set; }
		public string LdapPassword { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_MarkupTypeEmpty")]
		public string MarkupType { get; set; }
		public IEnumerable<string> MarkupTypesAvailable
		{
			get
			{
				return new string[] { "Creole","Markdown","MediaWiki" };
			}
		}
		
		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_ThemeEmpty")]
		public string Theme { get; set; }
		public IEnumerable<string> ThemesAvailable
		{
			get
			{
				if (string.IsNullOrEmpty(_themesRoot))
				{
					_themesRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Themes");
					if (!Directory.Exists(_themesRoot))
						throw new InvalidOperationException("The Themes directory could not be found");
				}

				foreach (string directory in Directory.GetDirectories(_themesRoot))
				{
					yield return new DirectoryInfo(directory).Name;
				}
			}
		}

		public string RecaptchaPrivateKey { get; set; }
		public string RecaptchaPublicKey { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_SiteNameEmpty")]
		public string SiteName { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_SiteUrlEmpty")]
		public string SiteUrl { get; set; }
	
		public bool UseWindowsAuth { get; set; }

		public string Version
		{
			get
			{
				return RoadkillSettings.Current.ApplicationSettings.Version;
			}
		}

		public SettingsSummary()
		{
			DatabaseType = DatabaseType.SqlServer2005;
			SiteUrl = "http://localhost";
		}

		public static SettingsSummary FromSystemSettings()
		{
			SettingsSummary summary = new SettingsSummary();

			summary.AdminRoleName = RoadkillSettings.Current.ApplicationSettings.AdminRoleName;
			summary.AllowedExtensions = string.Join(",", RoadkillSettings.Current.SitePreferences.AllowedFileTypes);
			summary.AllowUserSignup = RoadkillSettings.Current.SitePreferences.AllowUserSignup;
			summary.AttachmentsFolder = RoadkillSettings.Current.ApplicationSettings.AttachmentsFolder;
			summary.CacheEnabled = RoadkillSettings.Current.ApplicationSettings.CachedEnabled;
			summary.CacheText = RoadkillSettings.Current.ApplicationSettings.CacheText;
			summary.ConnectionString = RoadkillSettings.Current.ApplicationSettings.ConnectionString;
			summary.DatabaseType = RoadkillSettings.Current.ApplicationSettings.DatabaseType;
			summary.EditorRoleName = RoadkillSettings.Current.ApplicationSettings.EditorRoleName;
			summary.EnableRecaptcha = RoadkillSettings.Current.SitePreferences.IsRecaptchaEnabled;
			summary.LdapConnectionString = RoadkillSettings.Current.ApplicationSettings.LdapConnectionString;
			summary.LdapUsername = RoadkillSettings.Current.ApplicationSettings.LdapUsername;
			summary.LdapPassword = RoadkillSettings.Current.ApplicationSettings.LdapPassword;
			summary.MarkupType = RoadkillSettings.Current.SitePreferences.MarkupType;
			summary.RecaptchaPrivateKey = RoadkillSettings.Current.SitePreferences.RecaptchaPrivateKey;
			summary.RecaptchaPublicKey = RoadkillSettings.Current.SitePreferences.RecaptchaPublicKey;
			summary.SiteName = RoadkillSettings.Current.SitePreferences.SiteName;
			summary.SiteUrl = RoadkillSettings.Current.SitePreferences.SiteUrl;
			summary.Theme = RoadkillSettings.Current.SitePreferences.Theme;
			summary.UseWindowsAuth = RoadkillSettings.Current.ApplicationSettings.UseWindowsAuthentication;

			return summary;
		}
	}
}
