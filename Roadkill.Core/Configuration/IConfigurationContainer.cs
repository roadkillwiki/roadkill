using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// Defines a type that contains all system settings from both configuration files
	/// and database configuration storage.
	/// </summary>
	public interface IConfigurationContainer
	{
		/// <summary>
		/// Retrieves the configuration settings that are stored inside an application config 
		/// file and require an application restart when changed.
		/// </summary>
		/// <returns>A <see cref="RoadkillSection"/></returns>
		RoadkillSection ConfigurationFileSettings { get; set; }

		/// <summary>
		/// Retrieves the configuration settings that are stored in the database.
		/// </summary>
		/// <returns>A <see cref="SiteConfiguration"/></returns>
		SiteConfiguration DatabaseStoreSettings { get; set; }

		/// <summary>
		/// Refreshes all settings by getting the latest version from the database or configuration file.
		/// </summary>
		void Refresh();

		/// <summary>
		/// Whether users can register themselves, or if the administrators should do it. 
		/// If windows authentication is enabled, this setting is ignored.
		/// </summary>
		bool AllowUserSignup { get; set; }

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create,edit,delete pages,
		/// manage users, manage site settings and use the admin tools.
		/// </summary>
		string AdminRoleName { get; set; }

		/// <summary>
		/// Retrieves a list of the file extensions that are permitted for upload.
		/// </summary>
		IList<string> AllowedFileTypes { get; set; }

		/// <summary>
		/// The folder where all uploads (typically image files) are saved to. This is taken from the web.config,
		/// if the setting uses "~/" then the path is translated into one that is relative to the site root, 
		/// otherwise it is assumed to be an absolute file path.
		/// </summary>
		string AttachmentsFolder { get; set; }

		/// <summary>
		/// The route used for all attachment HTTP requests (currently non-user configurable). Should not contain a trailing slash.
		/// </summary>
		string AttachmentsUrlPath { get; set; }

		/// <summary>
		/// The route used for all attachment HTTP requests (currently non-user configurable), minus any starting "/".
		/// </summary>
		string AttachmentsRoutePath { get; set; }

		/// <summary>
		///  Indicates whether caching (currently NHibernate level 2 caching) is enabled.
		/// </summary>
		bool CachedEnabled { get; set; }

		/// <summary>
		/// Indicates whether textual content for pages is cached.
		/// </summary>
		bool CacheText { get; set; }

		/// <summary>
		/// The connection string to the Roadkill database.
		/// </summary>
		string ConnectionString { get; set; }

		/// <summary>
		/// The connection string name (held in the connection strings section of the config file) for the Roadkill database.
		/// </summary>
		string ConnectionStringName { get; set; }

		/// <summary>
		/// The database type used as the backing store.
		/// </summary>
		DatabaseType DatabaseType { get; set; }

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create and edit pages.
		/// </summary>
		string EditorRoleName { get; set; }

		/// <summary>
		/// Indicates whether the installation has been completed previously.
		/// </summary>
		bool Installed { get; set; }

		/// <summary>
		/// Whether the anti-spam Recaptcha service is enabled for signups and password resets.
		/// </summary>
		bool IsRecaptchaEnabled { get; set; }

		/// <summary>
		/// The connection string for Active Directory server if <see cref="UseWindowsAuthentication"/> is true.
		/// This should start with LDAP:// in uppercase.
		/// </summary>
		string LdapConnectionString { get; set; }

		/// <summary>
		/// The username to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		string LdapUsername { get; set; }

		/// <summary>
		/// The password to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		string LdapPassword { get; set; }

		/// <summary>
		/// The type of wiki markup the Roadkill installation is using. This can be three values: Creole, Markdown, MediaWiki.
		/// </summary>
		string MarkupType { get; set; }

		/// <summary>
		/// The number of characters each password should be.
		/// </summary>
		int MinimumPasswordLength { get; set; }

		/// <summary>
		/// The Recaptcha private key.
		/// </summary>
		string RecaptchaPrivateKey { get; set; }

		/// <summary>
		/// The Recaptcha public key.
		/// </summary>
		string RecaptchaPublicKey { get; set; }

		/// <summary>
		/// The name of the site, used by emails and themes.
		/// </summary>
		string SiteName { get; set; }

		/// <summary>
		/// The name of the site, used by emails.
		/// </summary>
		string SiteUrl { get; set; }

		/// <summary>
		/// The name of the theme for the wiki. This should be a folder in the ~/Themes/ directory inside the site root.
		/// </summary>
		string Theme { get; set; }

		/// <summary>
		/// The title of the wiki site, for use with themes.
		/// </summary>
		string Title { get; set; }

		/// <summary>
		/// An asp.net relativate path e.g. ~/Themes/ to the current theme directory. Does not include a trailing slash.
		/// </summary>
		string ThemePath { get; set; }

		/// <summary>
		/// Gets a value indicating whether this windows authentication is being used.
		/// </summary>
		bool UseWindowsAuthentication { get; set; }

		/// <summary>
		/// The current Roadkill version.
		/// </summary>
		string Version { get; set; }

		/// <summary>
		/// The path to the App data folder.
		/// </summary>
		string AppDataPath { get; set; }

		/// <summary>
		/// The file path for the custom tokens file.
		/// </summary>
		string CustomTokensPath { get; set; }

		/// <summary>
		/// The type for the <see cref="UserManager"/>. If the setting for this is blank
		/// in the web.config, then the <see cref="UseWindowsAuthentication"/> is checked and if false
		/// a <see cref="SqlUserManager"/> is created.
		/// </summary>
		string UserManagerType { get; set; }

		/// <summary>
		/// Whether errors in updating the lucene index throw exceptions or are just ignored.
		/// </summary>
		bool IgnoreSearchIndexErrors { get; set; }

		/// <summary>
		/// Whether the site is public, i.e. all pages are visible by default. This is optional in the web.config and the default is true.
		/// </summary>
		bool IsPublicSite { get; set; }

		/// <summary>
		/// Whether to scale images dynamically on the page, using Javascript, so they fit inside the main page container (400x400px).
		/// </summary>
		bool ResizeImages { get; set; }
	}
}
