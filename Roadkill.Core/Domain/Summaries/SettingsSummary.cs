using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Roadkill.Core.Localization.Resx;

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
				return RoadkillSettings.Current.Version;
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

			summary.AdminRoleName = RoadkillSettings.Current.AdminRoleName;
			summary.AllowedExtensions = string.Join(",", RoadkillSettings.Current.AllowedFileTypes);
			summary.AllowUserSignup = RoadkillSettings.Current.AllowUserSignup;
			summary.AttachmentsFolder = RoadkillSettings.Current.AttachmentsFolder;
			summary.CacheEnabled = RoadkillSettings.Current.CachedEnabled;
			summary.CacheText = RoadkillSettings.Current.CacheText;
			summary.ConnectionString = RoadkillSettings.Current.ConnectionString;
			summary.DatabaseType = RoadkillSettings.Current.DatabaseType;
			summary.EditorRoleName = RoadkillSettings.Current.EditorRoleName;
			summary.EnableRecaptcha = RoadkillSettings.Current.IsRecaptchaEnabled;
			summary.LdapConnectionString = RoadkillSettings.Current.LdapConnectionString;
			summary.LdapUsername = RoadkillSettings.Current.LdapUsername;
			summary.LdapPassword = RoadkillSettings.Current.LdapPassword;
			summary.MarkupType = RoadkillSettings.Current.MarkupType;
			summary.RecaptchaPrivateKey = RoadkillSettings.Current.RecaptchaPrivateKey;
			summary.RecaptchaPublicKey = RoadkillSettings.Current.RecaptchaPublicKey;
			summary.SiteName = RoadkillSettings.Current.Title;
			summary.SiteUrl = RoadkillSettings.Current.SiteUrl;
			summary.Theme = RoadkillSettings.Current.Theme;
			summary.UseWindowsAuth = RoadkillSettings.Current.UseWindowsAuthentication;

			return summary;
		}
	}
}
