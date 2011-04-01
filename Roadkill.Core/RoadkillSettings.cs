using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.Web.Configuration;
using System.Reflection;

namespace Roadkill.Core
{
	/// <summary>
	/// Holds setting information for both application and web.config settings for the Roadkill instance.
	/// </summary>
	public class RoadkillSettings
	{
		private static string _rolesConnectionString;
		private static string _ldapConnectionString;
		private static string _ldapUsername;
		private static string _ldapPassword;
		private static DatabaseType? _databaseType;

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
			get { return ConfigurationManager.ConnectionStrings[RoadkillSection.Current.ConnectionStringName].ConnectionString; }
		}

		/// <summary>
		/// The name of the role or Active Directory security group that users should belong to in order to create and edit pages.
		/// </summary>
		public static DatabaseType DatabaseType
		{
			get 
			{
				if (_databaseType == null)
				{
					DatabaseType dbType = DatabaseType.SqlServer;
					Enum.TryParse<DatabaseType>(RoadkillSection.Current.DatabaseType, true, out dbType);
					_databaseType = dbType;
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
		/// Gets a value indicating whether this windows authentication is being used.
		/// </summary>
		public static bool IsWindowsAuthentication
		{
			get
			{
				AuthenticationSection section = ConfigurationManager.GetSection("system.web/authentication") as AuthenticationSection;
				return section.Mode == AuthenticationMode.Windows;
			}
		}

		/// <summary>
		/// The connection string for Active Directory server if <see cref="IsWindowsAuthentication"/> is true.
		/// This should start with LDAP:// in uppercase.
		/// </summary>
		public static string LdapConnectionString
		{
			get
			{
				if (string.IsNullOrEmpty(_ldapConnectionString))
					_ldapConnectionString = GetRoleManagerConnectionString();

				return _ldapConnectionString;
			}
		}

		/// <summary>
		/// The username to authenticate against the Active Directory with, if <see cref="IsWindowsAuthentication"/> is true.
		/// </summary>
		public static string LdapUsername
		{
			get
			{
				if (string.IsNullOrEmpty(_ldapUsername))
					_ldapUsername = GetLdapConfigSetting("connectionUsername");

				return _ldapUsername;
			}
		}

		/// <summary>
		/// The password to authenticate against the Active Directory with, if <see cref="IsWindowsAuthentication"/> is true.
		/// </summary>
		public static string LdapPassword
		{
			get
			{
				if (string.IsNullOrEmpty(_ldapPassword))
					_ldapPassword = GetLdapConfigSetting("connectionPassword");

				return _ldapPassword;
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
		/// The connection string to the user/role database.
		/// </summary>
		public static string RolesConnectionString
		{
			get
			{
				if (string.IsNullOrEmpty(_rolesConnectionString))
					_rolesConnectionString = GetRoleManagerConnectionString();

				return _rolesConnectionString;
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
		/// Retrieves an LDAP setting from the system.web/rolemanager/provider element.
		/// </summary>
		private static string GetLdapConfigSetting(string name)
		{
			Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
			RoleManagerSection section = config.SectionGroups["system.web"].Sections["roleManager"] as RoleManagerSection;
			string defaultProvider = section.DefaultProvider;

			if (section.Providers.Count > 0 && section.Providers[defaultProvider].ElementInformation.Properties[name] != null)
				return section.Providers[defaultProvider].ElementInformation.Properties[name].Value.ToString();
			else
				return "";
		}

		/// <summary>
		/// Retrieves the roles connection string setting from the system.web/rolemanager/provider element.
		/// </summary>
		private static string GetRoleManagerConnectionString()
		{
			Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
			RoleManagerSection section = config.SectionGroups["system.web"].Sections["roleManager"] as RoleManagerSection;
			string defaultProvider = section.DefaultProvider;
			string connStringName = section.Providers[defaultProvider].ElementInformation.Properties["connectionStringName"].Value.ToString();

			string connectionString = "LDAP://";
			if (section.Providers.Count > 0 && section.Providers[defaultProvider].ElementInformation.Properties["connectionStringName"] != null)
				connectionString = config.ConnectionStrings.ConnectionStrings[connStringName].ConnectionString;

			return connectionString;
		}
	}
}