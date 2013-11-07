using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Contains all configuration data stored in the database, for settings that do not require an application restart when changed.
	/// This class is stored in the database as JSON.
	/// </summary>
	[Serializable]
	public class SiteSettings
	{
		internal static readonly Guid SiteSettingsId = new Guid("b960e8e5-529f-4f7c-aee4-28eb23e13dbd");
		private string _allowedFileTypes;
		private string _menuMarkup;

		#region Version 1.7
		/// <summary>
		/// The files types allowed for uploading.
		/// </summary>
		public string AllowedFileTypes
		{
			get
			{
				if (string.IsNullOrEmpty(_allowedFileTypes))
				{
					Log.Warn("The allowed file types setting is empty - populating with default types jpg, png, gif.");
					_allowedFileTypes = "jpg, png, gif";
				}

				return _allowedFileTypes;
			}
			set
			{
				_allowedFileTypes = value;
			}
		}

		/// <summary>
		/// Whether users can register themselves, or if the administrators should do it. 
		/// If windows authentication is enabled, this setting is ignored.
		/// </summary>
		public bool AllowUserSignup { get; set; }

		/// <summary>
		/// Whether to Recaptcha is enabled for user signups and password resets.
		/// </summary>
		public bool IsRecaptchaEnabled { get; set; }

		/// <summary>
		/// The type of markup used: Three available options are: Creole, Markdown, MediaWiki.
		/// The default is Creole.
		/// </summary>
		/// <remarks>This is a string because it's easier with the Javascript interaction.</remarks>
		public string MarkupType { get; set; }

		/// <summary>
		/// The private key for the recaptcha service, if enabled. This is optained when you sign up for the free service at https://www.google.com/recaptcha/.
		/// </summary>
		public string RecaptchaPrivateKey { get; set; }

		/// <summary>
		/// The public key for the recaptcha service, if enabled. This is optained when you sign up for the free service at https://www.google.com/recaptcha/.
		/// </summary>
		public string RecaptchaPublicKey { get; set; }

		/// <summary>
		/// The full url of the site.
		/// </summary>
		public string SiteUrl { get; set; }

		/// <summary>
		/// The title of the site.
		/// </summary>
		public string SiteName { get; set; }

		/// <summary>
		/// The site theme, defaults to "Blackbar"
		/// </summary>
		public string Theme { get; set; }

		/// <summary>
		/// An asp.net relativate path e.g. ~/Themes/ to the current theme directory. Does not include a trailing slash.
		/// </summary>
		[JsonIgnore]
		public string ThemePath
		{
			get
			{
				return string.Format("~/Themes/{0}", Theme);
			}
		}

		/// <summary>
		/// Retrieves a list of the file extensions that are permitted for upload.
		/// </summary>
		[JsonIgnore]
		public List<string> AllowedFileTypesList
		{
			get
			{
				return new List<string>(AllowedFileTypes.Replace(" ", "").Split(','));
			}
		}
		#endregion

		#region Version 2.0
		/// <summary>
		/// Whether files with the same name overwrite the existing file, or throw an error.
		/// </summary>
		public bool OverwriteExistingFiles { get; set; }

		/// <summary>
		/// Extra HTML/Javascript that is added to the HTML head, for example Google analytics, web fonts.
		/// </summary>
		public string HeadContent { get; set; }

		/// <summary>
		/// The left menu markup which is parsed and rendered.
		/// </summary>
		public string MenuMarkup
		{
			get
			{
				// If there's no menu markup (from an upgrade) default it.
				// Empty markup is valid, but null isn't.
				if (_menuMarkup == null)
					_menuMarkup = GetDefaultMenuMarkup();

				return _menuMarkup;
			}
			set
			{
				_menuMarkup = value;
			}
		}

		/// <summary>
		/// The last time a plugin was saved - this is used for 304 modified checks when browser caching is enabled.
		/// </summary>
		public DateTime PluginLastSaveDate { get; set; }
		#endregion

		public SiteSettings()
		{
			// v1.7
			AllowedFileTypes = "jpg, png, gif";
			AllowUserSignup = false;
			IsRecaptchaEnabled = false;
			Theme = "Mediawiki";
			MarkupType = "Creole";
			SiteName = "Your site";
			SiteUrl = "";
			RecaptchaPrivateKey = "";
			RecaptchaPublicKey = "";

			// v2.0
			OverwriteExistingFiles = false;
			HeadContent = "";
			MenuMarkup = GetDefaultMenuMarkup();
			PluginLastSaveDate = DateTime.UtcNow;
		}

		public string GetJson()
		{
			return JsonConvert.SerializeObject(this, Formatting.Indented);
		}

		internal string GetDefaultMenuMarkup()
		{
			return "* %mainpage%\r\n" +
					"* %categories%\r\n" +
					"* %allpages%\r\n" +
					"* %newpage%\r\n" +
					"* %managefiles%\r\n" +
					"* %sitesettings%\r\n\r\n";
		}

		public static SiteSettings LoadFromJson(string json)
		{
			if (string.IsNullOrEmpty(json))
			{
				Log.Warn("SiteSettings.LoadFromJson - json string was empty (returning a default SiteSettings object)");
				return new SiteSettings();
			}

			try
			{
				return JsonConvert.DeserializeObject<SiteSettings>(json);
			}
			catch (JsonReaderException ex)
			{
				Log.Error(ex, "SiteSettings.LoadFromJson - an exception occurred deserializing the JSON");
				return new SiteSettings();
			}
		}
	}
}
