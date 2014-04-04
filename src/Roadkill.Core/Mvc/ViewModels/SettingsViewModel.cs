using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// Represents settings for the site, some of which are stored in the web.config.
	/// </summary>
	[Serializable]
	public class SettingsViewModel
	{
		private static string _themesRoot;

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_MarkupTypeEmpty")]
		public string MarkupType { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_SiteNameEmpty")]
		public string SiteName { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_SiteUrlEmpty")]
		public string SiteUrl { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_AttachmentsEmpty")]
		[RegularExpression(@"^[^/Files].*", ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_AttachmentsReservedName")]
		public string AttachmentsFolder { get; set; }

		public string AzureConnectionString { get; set; }

		public string AzureContainer { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_ConnectionEmpty")]
		public string ConnectionString { get; set; }

		[Required(ErrorMessageResourceType = typeof(SiteStrings), ErrorMessageResourceName = "SiteSettings_Validation_ThemeEmpty")]
		public string Theme { get; set; }

		public string AdminEmail { get; set; }
		public string AdminPassword { get; set; }
		public string AdminRoleName { get; set; }
		public string AllowedFileTypes { get; set; }
		public bool AllowUserSignup { get; set; }
		public string AttachmentsDirectoryPath { get; set; }
		public bool UseObjectCache { get; set; }
		public bool UseBrowserCache { get; set; }
		public string DataStoreTypeName { get; set; }
		public string EditorRoleName { get; set; }
		public bool IsRecaptchaEnabled { get; set; }
		public string LdapConnectionString { get; set; }
		public string LdapUsername { get; set; }
		public string LdapPassword { get; set; }
		public string RecaptchaPrivateKey { get; set; }
		public string RecaptchaPublicKey { get; set; }
		public bool UseAzureFileStorage { get; set; }
		public bool UseWindowsAuth { get; set; }
		
		// v2.0
		public bool OverwriteExistingFiles { get; set; }
		public string HeadContent { get; set; }
		public string MenuMarkup { get; set; }

		public bool IsPublicSite { get; set; }
		public bool IgnoreSearchIndexErrors { get; set; }

		/// <summary>
		/// True when the model was updated during postback
		/// </summary>
		public bool UpdateSuccessful { get; set; }

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

		// TODO: tests
		/// <summary>
		/// Gets an IEnumerable{SelectListItem} from a the SettingsViewModel.DatabaseTypesAvailable, as a default
		/// SelectList doesn't add option value attributes.
		/// </summary>
		public List<SelectListItem> DatabaseTypesAsSelectList
		{
			get
			{
				List<SelectListItem> items = new List<SelectListItem>();

				foreach (string name in DatabaseTypesAvailable)
				{
					SelectListItem item = new SelectListItem();
					item.Text = name;
					item.Value = name;

					if (name == DataStoreTypeName)
						item.Selected = true;

					items.Add(item);
				}

				return items;
			}
		}

		public IEnumerable<string> MarkupTypesAvailable
		{
			get
			{
				return new string[] { "Creole","Markdown","MediaWiki" };
			}
		}

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

		public string Version
		{
			get
			{
				return ApplicationSettings.ProductVersion;
			}
		}

		public SettingsViewModel()
		{
			if (HttpContext.Current != null)
			{
				// Default the site's url using the current request
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

		/// <summary>
		/// Fills this instance of SettingsViewModel using the properties from the ApplicationSettings 
		/// and the SiteSettings.
		/// </summary>
		public SettingsViewModel(ApplicationSettings applicationSettings, SiteSettings siteSettings) : this()
		{
			// ApplicationSettings
			FillFromApplicationSettings(applicationSettings);

			// SiteSettings
			AllowedFileTypes = string.Join(",", siteSettings.AllowedFileTypesList);
			AllowUserSignup = siteSettings.AllowUserSignup;
			IsRecaptchaEnabled = siteSettings.IsRecaptchaEnabled;
			MarkupType = siteSettings.MarkupType;
			RecaptchaPrivateKey = siteSettings.RecaptchaPrivateKey;
			RecaptchaPublicKey = siteSettings.RecaptchaPublicKey;
			SiteName = siteSettings.SiteName;
			SiteUrl = siteSettings.SiteUrl;
			Theme = siteSettings.Theme;
			OverwriteExistingFiles = siteSettings.OverwriteExistingFiles;
			HeadContent = siteSettings.HeadContent;
			MenuMarkup = siteSettings.MenuMarkup;
		}

		public void FillFromApplicationSettings(ApplicationSettings applicationSettings)
		{
			AdminRoleName = applicationSettings.AdminRoleName;
			AttachmentsFolder = applicationSettings.AttachmentsFolder;
			AttachmentsDirectoryPath = applicationSettings.AttachmentsDirectoryPath;
			ConnectionString = applicationSettings.ConnectionString;
			DataStoreTypeName = applicationSettings.DataStoreType.Name;
			EditorRoleName = applicationSettings.EditorRoleName;
			IsPublicSite = applicationSettings.IsPublicSite;
			IgnoreSearchIndexErrors = applicationSettings.IgnoreSearchIndexErrors;
			LdapConnectionString = applicationSettings.LdapConnectionString;
			LdapUsername = applicationSettings.LdapUsername;
			LdapPassword = applicationSettings.LdapPassword;
			UseWindowsAuth = applicationSettings.UseWindowsAuthentication;
			UseObjectCache = applicationSettings.UseObjectCache;
			UseBrowserCache = applicationSettings.UseBrowserCache;
		}
	}
}
