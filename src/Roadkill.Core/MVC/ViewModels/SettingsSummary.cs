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

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// Represents settings for the site, some of which are stored in the web.config.
	/// </summary>
	[Serializable]
	public class SettingsSummary
	{
		private static string _themesRoot;

		public SettingsSummary()
		{
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
		public string AllowedFileTypes { get; set; }
		public bool AllowUserSignup { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_AttachmentsEmpty")]
		[RegularExpression(@"^[^/Files].*",ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_AttachmentsReservedName")]
		public string AttachmentsFolder { get; set; }

		public string AttachmentsDirectoryPath { get; set; }

		public bool UseObjectCache { get; set; }
		public bool UseBrowserCache { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_ConnectionEmpty")]
		public string ConnectionString { get; set; }

		public string DataStoreTypeName { get; set; }
		public IEnumerable<string> DatabaseTypesAvailable
		{
			get
			{
#if MONO
				return DataStoreType.AllMonoTypes.Select(x => x.Name);
#else
				return DataStoreType.AllTypes.Select(x => x.Name);
#endif
			}
		}

		public string EditorRoleName { get; set; }
		public bool IsRecaptchaEnabled { get; set; }
		
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
				return ApplicationSettings.ProductVersion;
			}
		}

		public void FillFromApplicationSettings(ApplicationSettings applicationSettings)
		{
			AdminRoleName = applicationSettings.AdminRoleName;
			AttachmentsFolder = applicationSettings.AttachmentsFolder;
			AttachmentsDirectoryPath = applicationSettings.AttachmentsDirectoryPath;
			UseObjectCache = applicationSettings.UseObjectCache;
			UseBrowserCache = applicationSettings.UseBrowserCache;
			ConnectionString = applicationSettings.ConnectionString;
			DataStoreTypeName = applicationSettings.DataStoreType.Name;
			EditorRoleName = applicationSettings.EditorRoleName;
			LdapConnectionString = applicationSettings.LdapConnectionString;
			LdapUsername = applicationSettings.LdapUsername;
			LdapPassword = applicationSettings.LdapPassword;
			UseWindowsAuth = applicationSettings.UseWindowsAuthentication;
		}

		public void FillFromSiteSettings(SiteSettings siteSettings)
		{
			AllowedFileTypes = string.Join(",", siteSettings.AllowedFileTypesList);
			AllowUserSignup = siteSettings.AllowUserSignup;
			IsRecaptchaEnabled = siteSettings.IsRecaptchaEnabled;
			MarkupType = siteSettings.MarkupType;
			RecaptchaPrivateKey = siteSettings.RecaptchaPrivateKey;
			RecaptchaPublicKey = siteSettings.RecaptchaPublicKey;
			SiteName = siteSettings.SiteName;
			SiteUrl = siteSettings.SiteUrl;
			Theme = siteSettings.Theme;
		}
	}
}
