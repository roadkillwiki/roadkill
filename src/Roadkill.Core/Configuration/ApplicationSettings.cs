using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.Web.Configuration;
using System.Reflection;
using System.IO;
using StructureMap;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;
using System.Diagnostics;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Contains all settings that require an application (appdomain) restart when changed - typically stored in a .config file.
	/// </summary>
	public class ApplicationSettings
	{
		private string _attachmentsFolder;
		private string _attachmentsDirectoryPath;
		private string _attachmentsUrlPath;
		private string _attachmentsRoutePath;
		private HttpContextBase _httpContext;

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create,edit,delete pages,
		/// manage users, manage site settings and use the admin tools.
		/// </summary>
		public string AdminRoleName { get; set; }

		/// <summary>
		/// The path to the App_Data folder.
		/// </summary>
		public string AppDataPath { get; set; }

		/// <summary>
		/// The path to the App_Data/Internal folder (used by roadkill only, no user files are stored here).
		/// </summary>
		public string AppDataInternalPath { get; private set; }

		/// <summary>
		/// The path to the folder that contains the Lucene index - ~/App_Data/Internal/Search.
		/// </summary>
		public string SearchIndexPath { get; set; }

		/// <summary>
		/// The path to the folder that contains x86 and x64 SQLite binary file - ~/App_Data/Internal/SQLiteBinaries.
		/// </summary>
		public string SQLiteBinariesPath { get; set; }

		/// <summary>
		/// The folder where all uploads (typically image files) are saved to. This is taken from the web.config.
		/// Use AttachmentsDirectoryPath for the absolute directory path.
		/// </summary>
		public string AttachmentsFolder
		{
			get
			{
				return _attachmentsFolder;
			}
			set
			{
				_attachmentsFolder = value;
				_attachmentsDirectoryPath = "";
			}
		}

		/// <summary>
		/// The absolute file path for the attachments folder. If the AttachmentsFolder uses "~/" then the path is 
		/// translated into one that is relative to the site root, otherwise it is assumed to be an absolute file path.
		/// This property always contains a trailing slash (or / on Unix based systems).
		/// </summary>
		public string AttachmentsDirectoryPath
		{
			get
			{
				if (string.IsNullOrEmpty(_attachmentsDirectoryPath))
				{
					if (AttachmentsFolder.StartsWith("~") && _httpContext != null)
					{
						_attachmentsDirectoryPath = _httpContext.Server.MapPath(AttachmentsFolder);
					}
					else
					{
						_attachmentsDirectoryPath = AttachmentsFolder;
					}
				}

				if (!_attachmentsDirectoryPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
					_attachmentsDirectoryPath += Path.DirectorySeparatorChar.ToString();

				return _attachmentsDirectoryPath;
			}
		}

		/// <summary>
		/// Gets the full path for the attachments folder, including any extra application paths from the url.
		/// Contains a "/" the start and does not contain a trailing "/".
		/// </summary>
		public string AttachmentsUrlPath
		{
			get
			{
				if (string.IsNullOrEmpty(_attachmentsUrlPath))
				{
					_attachmentsUrlPath = ParseAttachmentsPath();
				}

				return _attachmentsUrlPath;
			}
		}

		/// <summary>
		/// The route used for all attachment HTTP requests. This contains no starting or ending "/".
		/// </summary>
		public string AttachmentsRoutePath
		{
			get
			{
				return _attachmentsRoutePath;
			}
			set
			{
				_attachmentsRoutePath = value;
				_attachmentsUrlPath = "";
			}
		}

		/// <summary>
		/// The connection string to the Roadkill database.
		/// </summary>
		public string ConnectionString { get; set; }

		/// <summary>
		/// The connection string name (held in the connection strings section of the config file) for the Roadkill database.
		/// </summary>
		public string ConnectionStringName { get; set; }

		/// <summary>
		/// The file path for the custom tokens file.
		/// </summary>
		public string CustomTokensPath { get; set; }

		/// <summary>
		/// The full path to the text plugins directory. This is where plugins are stored after 
		/// download (including their nuget files), and are copied to the bin folder.
		/// </summary>
		public string PluginsPath { get; internal set; }

		/// <summary>
		/// The directory within the /bin folder that the plugins are stored. They are 
		/// copied here on application start, so they can be loaded into the application domain with shadow 
		/// copy support and also monitored by the ASP.NET file watcher.
		/// </summary>
		public string PluginsBinPath { get; internal set; }

		/// <summary>
		/// The database type used as the backing store.
		/// </summary>
		public DataStoreType DataStoreType { get; set; }

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create and edit pages.
		/// </summary>
		public string EditorRoleName { get; set; }

		/// <summary>
		/// The path to the email templates folder, ~/App_Data/EmailTemplates/ by default.
		/// </summary>
		public string EmailTemplateFolder { get; set; }

		/// <summary>
		/// The file path for the html element white list file.
		/// </summary>
		public string HtmlElementWhiteListPath { get; set; }

		/// <summary>
		/// Whether errors in updating the lucene index throw exceptions or are just ignored.
		/// </summary>
		public bool IgnoreSearchIndexErrors { get; set; }

		/// <summary>
		/// Whether the site is public, i.e. all pages are visible by default. This is optional in the web.config and the default is true.
		/// </summary>
		public bool IsPublicSite { get; set; }

		/// <summary>
		/// If this instance is running on the demo site.
		/// </summary>
		internal bool IsDemoSite
		{
			get
			{
				return ConfigurationManager.AppSettings["DemoSite"] == "true";
			}
		}

		/// <summary>
		/// Indicates whether the installation has been completed previously.
		/// </summary>
		public bool Installed { get; set; }

		/// <summary>
		/// The connection string for Active Directory server if <see cref="UseWindowsAuthentication"/> is true.
		/// This should start with LDAP:// in uppercase.
		/// </summary>
		public string LdapConnectionString { get; set; }

		/// <summary>
		/// The username to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		public string LdapUsername { get; set; }

		/// <summary>
		/// The password to authenticate against the Active Directory with, if <see cref="UseWindowsAuthentication"/> is true.
		/// </summary>
		public string LdapPassword { get; set; }

		/// <summary>
		/// The type of logging to perform, which can be a comma seperated list of values or just a single value.
		/// </summary>
		public string LoggingTypes { get; set; }

		/// <summary>
		/// Whether to just error messages are logged, or all information (warnings, information).
		/// </summary>
		public bool LogErrorsOnly { get; set; }

		/// <summary>
		/// The number of characters each password should be.
		/// </summary>
		public int MinimumPasswordLength { get; set; }

		/// <summary>
		/// The fully qualified assembly and classname for the repository.
		/// </summary>
		public string RepositoryType { get; set; }

		/// <summary>
		/// True if the version number in the web.config does not match the current assembly version.
		/// </summary>
		public bool UpgradeRequired { get; internal set; }

		/// <summary>
		/// Indicates whether server-based page object caching is enabled.
		/// </summary>
		public bool UseObjectCache { get; set; }

		/// <summary>
		/// Indicates whether to send HTTP cache headers to the browser (304 not modified)
		/// </summary>
		public bool UseBrowserCache { get; set; }

		/// <summary>
		/// Gets a value indicating whether the html that is converted from the markup is 
		/// cleaned for tags, using the App_Data/htmlwhitelist.xml file.
		/// </summary>
		public bool UseHtmlWhiteList { get; set; }

		/// <summary>
		/// The type for the <see cref="UserServiceBase"/>. If the setting for this is blank
		/// in the web.config, then the <see cref="UseWindowsAuthentication"/> is checked and if false
		/// a <see cref="FormsAuthUserService"/> is created. The format of this setting can be retrieved by
		/// using <code>typeof(YourUserService).FullName.</code>
		/// </summary>
		public string UserServiceType { get; set; }

		/// <summary>
		/// Gets a value indicating whether this windows authentication is being used.
		/// </summary>
		public bool UseWindowsAuthentication { get; set; }

		/// <summary>
		/// The human-friendly current Roadkill product version, e.g. "1.7.0-Beta3".
		/// </summary>
		public static string ProductVersion
		{
			get
			{
				return FileVersionInfo.GetVersionInfo(typeof(ApplicationSettings).Assembly.Location).ProductVersion;
			}
		}

		/// <summary>
		/// The file version of the Roadkill product version, e.g. "1.7.0.0"
		/// </summary>
		public static string FileVersion
		{
			get
			{
				return FileVersionInfo.GetVersionInfo(typeof(ApplicationSettings).Assembly.Location).FileVersion;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
		/// </summary>
		public ApplicationSettings()
		{
			if (HttpContext.Current != null)
				_httpContext = new HttpContextWrapper(HttpContext.Current);

			AppDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
			AppDataInternalPath = Path.Combine(AppDataPath, "Internal");
			CustomTokensPath = Path.Combine(AppDataPath, "customvariables.xml");
			EmailTemplateFolder = Path.Combine(AppDataPath, "EmailTemplates");
			HtmlElementWhiteListPath = Path.Combine(AppDataInternalPath, "htmlwhitelist.xml");
			MinimumPasswordLength = 6;
			DataStoreType = DataStoreType.SqlServer2008;
			AttachmentsRoutePath = "Attachments";
			AttachmentsFolder = "~/App_Data/Attachments";
			SearchIndexPath = Path.Combine(AppDataInternalPath, "Search");
			SQLiteBinariesPath = Path.Combine(AppDataInternalPath, "SQLiteBinaries");
			PluginsBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "Plugins");
			PluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ApplicationSettings"/> class.
		/// </summary>
		/// <param name="httpContext">The HTTP context.</param>
		internal ApplicationSettings(HttpContextBase httpContext) : this()
		{
			_httpContext = httpContext;
		}

		private string ParseAttachmentsPath()
		{
			string attachmentsPath = "/" + AttachmentsRoutePath;
			if (_httpContext != null)
			{
				string applicationPath = _httpContext.Request.ApplicationPath;
				if (!applicationPath.EndsWith("/"))
					applicationPath += "/";

				if (attachmentsPath.StartsWith("/"))
					attachmentsPath = attachmentsPath.Remove(0, 1);

				attachmentsPath = applicationPath + attachmentsPath;
			}

			return attachmentsPath;
		}
	}
}