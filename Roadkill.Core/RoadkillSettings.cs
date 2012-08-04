using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.Web.Configuration;
using System.Reflection;
using System.IO;

namespace Roadkill.Core
{
	/// <summary>
	/// Holds setting information for both application and web.config settings for the Roadkill instance.
	/// </summary>
	/// <remarks>This class acts as a helper for RoadkillSection and SiteConfiguration as a single point for all settings.</remarks>
	public class RoadkillSettings
	{
		internal static DatabaseType? _databaseType = null;
		internal static string _connectionString;

		/// <summary>
		/// Whether users can register themselves, or if the administrators should do it. 
		/// If windows authentication is enabled, this setting is ignored.
		/// </summary>
		public static bool AllowUserSignup
		{
			get
			{
				return SiteConfiguration.Current.AllowUserSignup;
			}
		}

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create,edit,delete pages,
		/// manage users, manage site settings and use the admin tools.
		/// </summary>
		public static string AdminRoleName
		{
			get { return RoadkillSection.Current.AdminRoleName; }
		}

		/// <summary>
		/// Retrieves a list of the file extensions that are permitted for upload.
		/// </summary>
		public static IList<string> AllowedFileTypes
		{
			get
			{
				return new List<string>(SiteConfiguration.Current.AllowedFileTypes.Split(','));
			}
		}

		/// <summary>
		/// The folder where all uploads (typically image files) are saved to. This should start with "~/" to indicate the site root.
		/// </summary>
		public static string AttachmentsFolder
		{
			get { return RoadkillSection.Current.AttachmentsFolder; }
		}

		/// <summary>
		///  Indicates whether caching (currently NHibernate level 2 caching) is enabled.
		/// </summary>
		public static bool CachedEnabled
		{
			get { return RoadkillSection.Current.CacheEnabled; }
		}

		/// <summary>
		/// Indicates whether textual content for pages is cached.
		/// </summary>
		public static bool CacheText
		{
			get { return RoadkillSection.Current.CacheText; }
		}

		/// <summary>
		/// The connection string to the Roadkill page database.
		/// </summary>
		public static string ConnectionString
		{
			get 
			{ 
				if (string.IsNullOrEmpty(_connectionString))
					_connectionString = ConfigurationManager.ConnectionStrings[RoadkillSection.Current.ConnectionStringName].ConnectionString;

				return _connectionString; 
			}
			set
			{
				_connectionString = value;
			}
		}

		/// <summary>
		/// The database type used as the backing store.
		/// </summary>
		public static DatabaseType DatabaseType
		{
			get 
			{
				if (_databaseType == null)
				{
					if (string.IsNullOrEmpty(RoadkillSection.Current.DatabaseType))
						return DatabaseType.SqlServer2005;

					DatabaseType dbType;

					if (Enum.TryParse<DatabaseType>(RoadkillSection.Current.DatabaseType, true, out dbType))
						_databaseType = dbType;
					else
						_databaseType = DatabaseType.SqlServer2005;
				}

				return _databaseType.Value;
			}
		}

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create and edit pages.
		/// </summary>
		public static string EditorRoleName
		{
			get { return RoadkillSection.Current.EditorRoleName; }
		}

		/// <summary>
		/// Indicates whether the installation has been completed previously.
		/// </summary>
		public static bool Installed
		{
			get { return RoadkillSection.Current.Installed; }
		}

		/// <summary>
		/// Whether the anti-spam Recaptcha service is enabled for signups and password resets.
		/// </summary>
		public static bool IsRecaptchaEnabled
		{
			get { return SiteConfiguration.Current.EnableRecaptcha; }
		}

		/// <summary>
		/// The connection string for Active Directory server if <see cref="UseWindowsAuthentication"/> is true.
		/// This should start with LDAP:// in uppercase.
		/// </summary>
		public static string LdapConnectionString
		{
			get
			{
				return RoadkillSection.Current.LdapConnectionString;
			}
		}

		/// <summary>
		/// The username to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		public static string LdapUsername
		{
			get
			{
				return RoadkillSection.Current.LdapUsername;
			}
		}

		/// <summary>
		/// The password to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		public static string LdapPassword
		{
			get
			{
				return RoadkillSection.Current.LdapPassword;
			}
		}

		/// <summary>
		/// The type of wiki markup the Roadkill installation is using. This can be three values: Creole, Markdown, MediaWiki.
		/// </summary>
		public static string MarkupType
		{
			get { return SiteConfiguration.Current.MarkupType; }
		}

		/// <summary>
		/// The number of characters each password should be.
		/// </summary>
		public static int MinimumPasswordLength
		{
			get { return 6; }
		}

		/// <summary>
		/// The Recaptcha private key.
		/// </summary>
		public static string RecaptchaPrivateKey
		{
			get
			{
				return SiteConfiguration.Current.RecaptchaPrivateKey;
			}
		}

		/// <summary>
		/// The Recaptcha public key.
		/// </summary>
		public static string RecaptchaPublicKey
		{
			get
			{
				return SiteConfiguration.Current.RecaptchaPublicKey;
			}
		}

	
		/// <summary>
		/// The name of the site, used by emails and themes.
		/// </summary>
		public static string SiteName
		{
			get
			{
				return SiteConfiguration.Current.Title;
			}
		}

		/// <summary>
		/// The name of the site, used by emails.
		/// </summary>
		public static string SiteUrl
		{
			get
			{
				return SiteConfiguration.Current.SiteUrl;
			}
		}

		/// <summary>
		/// The name of the theme for the wiki. This should be a folder in the ~/Themes/ directory inside the site root.
		/// </summary>
		public static string Theme
		{
			get { return SiteConfiguration.Current.Theme; }
		}

		/// <summary>
		/// The title of the wiki site, for use with themes.
		/// </summary>
		public static string Title
		{
			get { return SiteConfiguration.Current.Title; }
		}

		/// <summary>
		/// An asp.net relativate path e.g. ~/Themes/ to the current theme directory. Does not include a trailing slash.
		/// </summary>
		public static string ThemePath
		{
			get
			{
				return string.Format("~/Themes/{0}", Theme);
			}
		}

		/// <summary>
		/// Gets a value indicating whether this windows authentication is being used.
		/// </summary>
		public static bool UseWindowsAuthentication
		{
			get
			{
				return RoadkillSection.Current.UseWindowsAuthentication;
			}
		}

		/// <summary>
		/// The current Roadkill version.
		/// </summary>
		public static string Version
		{
			get
			{
				return typeof(RoadkillSettings).Assembly.GetName().Version.ToString();
			}
		}

		/// <summary>
		/// The path to the App data folder.
		/// </summary>
		public static string AppDataPath
		{
			get
			{
				return AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\";
			}
		}

		/// <summary>
		/// The file path for the custom tokens file.
		/// </summary>
		public static string CustomTokensPath
		{
			get
			{
				return Path.Combine(AppDataPath, "tokens.xml");
			}
		}

		/// <summary>
		/// The type for the <see cref="UserManager"/>. If the setting for this is blank
		/// in the web.config, then the <see cref="UseWindowsAuthentication"/> is checked and if false
		/// a <see cref="SqlUserManager"/> is created.
		/// </summary>
		public static string UserManagerType
		{
			get
			{
				return RoadkillSection.Current.UserManagerType;
			}
		}

		/// <summary>
		/// Whether errors in updating the lucene index throw exceptions or are just ignored.
		/// </summary>
		public static bool IgnoreSearchIndexErrors
		{
			get
			{
				return RoadkillSection.Current.IgnoreSearchIndexErrors;
			}
		}
	}
}