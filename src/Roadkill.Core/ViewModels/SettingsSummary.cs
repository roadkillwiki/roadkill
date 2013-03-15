using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Configuration;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Database;

namespace Roadkill.Core
{
	/// <summary>
	/// Represents settings for the site, some of which are stored in the web.config.
	/// </summary>
	[Serializable]
	public class SettingsSummary
	{
		private static string _themesRoot;
		protected IConfigurationContainer Config;

		public SettingsSummary(IConfigurationContainer config)
		{
			Config = config;
			//DataStoreType = DataStoreType.SqlServer2005;

			if (HttpContext.Current != null)
			{
				Uri uri = HttpContext.Current.Request.Url;

				string port = "";
				if (uri.Port != 80 && uri.Port != 443)
					port = ":" + uri.Port;

				SiteUrl = string.Format("{0}://{1}{2}", uri.Scheme, uri.Host, port);
			}
			else
			{
				SiteUrl = "http://localhost";
			}
		}

		public string AdminEmail { get; set; }
		public string AdminPassword { get; set; }
		public string AdminRoleName { get; set; }
		public string AllowedExtensions { get; set; }
		public bool AllowUserSignup { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_AttachmentsEmpty")]
		[RegularExpression(@"^[^/Files].*", ErrorMessage = "'~/Files' is a reserved path, please choose another attachments folder.")]
		public string AttachmentsFolder { get; set; }

		public bool CacheEnabled { get; set; }
		public bool CacheText { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_ConnectionEmpty")]
		public string ConnectionString { get; set; }

		public string DataStoreTypeName { get; set; }
		public IEnumerable<string> DatabaseTypesAvailable
		{
			get
			{
				return DataStoreType.AllTypes.Select(x => x.Name);
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
				return ApplicationSettings.Version.ToString();
			}
		}

		/// <summary>
		/// Converts the current configuration settings from the databaes into a SettingsSummary view model.
		/// </summary>
		public static SettingsSummary FromSystemSettings(IConfigurationContainer config)
		{
			SettingsSummary summary = new SettingsSummary(config);

			summary.AdminRoleName = config.ApplicationSettings.AdminRoleName;
			summary.AllowedExtensions = string.Join(",", config.SitePreferences.AllowedFileTypes);
			summary.AllowUserSignup = config.SitePreferences.AllowUserSignup;
			summary.AttachmentsFolder = config.ApplicationSettings.AttachmentsFolder;
			summary.CacheEnabled = config.ApplicationSettings.CacheEnabled;
			summary.CacheText = config.ApplicationSettings.CacheText;
			summary.ConnectionString = config.ApplicationSettings.ConnectionString;
			summary.DataStoreTypeName = config.ApplicationSettings.DataStoreType.Name;
			summary.EditorRoleName = config.ApplicationSettings.EditorRoleName;
			summary.EnableRecaptcha = config.SitePreferences.IsRecaptchaEnabled;
			summary.LdapConnectionString = config.ApplicationSettings.LdapConnectionString;
			summary.LdapUsername = config.ApplicationSettings.LdapUsername;
			summary.LdapPassword = config.ApplicationSettings.LdapPassword;
			summary.MarkupType = config.SitePreferences.MarkupType;
			summary.RecaptchaPrivateKey = config.SitePreferences.RecaptchaPrivateKey;
			summary.RecaptchaPublicKey = config.SitePreferences.RecaptchaPublicKey;
			summary.SiteName = config.SitePreferences.SiteName;
			summary.SiteUrl = config.SitePreferences.SiteUrl;
			summary.Theme = config.SitePreferences.Theme;
			summary.UseWindowsAuth = config.ApplicationSettings.UseWindowsAuthentication;

			return summary;
		}
	}
}
