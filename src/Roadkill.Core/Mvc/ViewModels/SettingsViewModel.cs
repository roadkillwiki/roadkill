using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Localization;
using System;
using Roadkill.Core.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// Represents settings for the site, some of which are stored in the web.config.
	/// </summary>
	[Serializable]
	public class SettingsViewModel
	{
		private static string _themesRoot;
		private List<SelectListItem> _supportedDatabasesSelectList;

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
		public string DatabaseName { get; set; }
		public string EditorRoleName { get; set; }
		public bool IsRecaptchaEnabled { get; set; }
		public string RecaptchaPrivateKey { get; set; }
		public string RecaptchaPublicKey { get; set; }
		public bool UseAzureFileStorage { get; set; }
		
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


		/// <summary>
		/// Gets an IEnumerable{SelectListItem} from a the SettingsViewModel.DatabaseTypesAvailable, as a default
		/// SelectList doesn't add option value attributes.
		/// </summary>
		public List<SelectListItem> DatabaseTypesAsSelectList
		{
			get
			{
				return _supportedDatabasesSelectList;
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
					// The themes are in the content root (not the bin folder) with ASP.NET Core.
					ApplicationSettings applicationSettings = HttpContextHolder.Current?.RequestServices.GetService(typeof(ApplicationSettings)) as ApplicationSettings;
					string contentRoot = applicationSettings?.ContentRootPath ?? AppContext.BaseDirectory;
					_themesRoot = Path.Combine(contentRoot, "Themes");
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
			if (HttpContextHolder.Current != null)
			{
				// Default the site's url using the current request
				Microsoft.AspNetCore.Http.HttpRequest request = HttpContextHolder.Current.Request;
				SiteUrl = string.Format("{0}://{1}", request.Scheme, request.Host);
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
			DatabaseName = applicationSettings.DatabaseName;
			EditorRoleName = applicationSettings.EditorRoleName;
			IsPublicSite = applicationSettings.IsPublicSite;
			IgnoreSearchIndexErrors = applicationSettings.IgnoreSearchIndexErrors;
			UseObjectCache = applicationSettings.UseObjectCache;
			UseBrowserCache = applicationSettings.UseBrowserCache;
		}

		public void SetSupportedDatabases(IEnumerable<RepositoryInfo> repositoryInfos)
		{
			_supportedDatabasesSelectList = new List<SelectListItem>();

			foreach (RepositoryInfo info in repositoryInfos)
			{
				SelectListItem item = new SelectListItem();
				item.Value = info.Id;
				item.Text = info.Description;

				if (item.Value == DatabaseName)
					item.Selected = true;

				_supportedDatabasesSelectList.Add(item);
			}
		}
	}
}
