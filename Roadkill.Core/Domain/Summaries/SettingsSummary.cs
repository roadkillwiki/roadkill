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

		public string EditorRoleName { get; set; }
		public bool EnableRecaptcha { get; set; }
		
		public string LdapConnectionString { get; set; }
		public string LdapUsername { get; set; }
		public string LdapPassword { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_MarkupTypeEmpty")]
		public string MarkupType { get; set; }
		
		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_ThemeEmpty")]
		public string Theme { get; set; }

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
				return RoadkillSettings.Version;
			}
		}

		public SettingsSummary()
		{
			DatabaseType = DatabaseType.SqlServer2005;
			SiteUrl = "http://localhost";
		}

		public static SettingsSummary GetCurrentSettings()
		{
			SettingsSummary summary = new SettingsSummary();
			
			summary.AdminRoleName = RoadkillSettings.AdminRoleName;
			summary.AllowedExtensions = string.Join(",",RoadkillSettings.AllowedFileTypes);
			summary.AllowUserSignup = SiteConfiguration.Current.AllowUserSignup;
			summary.AttachmentsFolder = RoadkillSettings.AttachmentsFolder;
			summary.CacheEnabled = RoadkillSettings.CachedEnabled;
			summary.CacheText = RoadkillSettings.CacheText;
			summary.ConnectionString = RoadkillSettings.ConnectionString;
			summary.DatabaseType = RoadkillSettings.DatabaseType;
			summary.EditorRoleName = RoadkillSettings.EditorRoleName;
			summary.EnableRecaptcha = SiteConfiguration.Current.EnableRecaptcha;
			summary.LdapConnectionString = RoadkillSettings.LdapConnectionString;
			summary.LdapUsername = RoadkillSettings.LdapUsername;
			summary.LdapPassword = RoadkillSettings.LdapPassword;
			summary.MarkupType = RoadkillSettings.MarkupType;
			summary.RecaptchaPrivateKey = SiteConfiguration.Current.RecaptchaPrivateKey;
			summary.RecaptchaPublicKey = SiteConfiguration.Current.RecaptchaPublicKey;
			summary.SiteName = SiteConfiguration.Current.Title;
			summary.SiteUrl = SiteConfiguration.Current.SiteUrl;
			summary.Theme = RoadkillSettings.Theme;
			summary.UseWindowsAuth = RoadkillSettings.UseWindowsAuthentication;

			return summary;
		}
	}
}
