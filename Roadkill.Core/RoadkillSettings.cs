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
	/// Holds information for both application and web.config settings for the Roadkill instance.
	/// </summary>
	public class RoadkillSettings
	{
		private static string _rolesConnectionString;
		private static string _ldapConnectionString;
		private static string _ldapUsername;
		private static string _ldapPassword;

		public static bool IsWindowsAuthentication
		{
			get
			{
				AuthenticationSection section = ConfigurationManager.GetSection("system.web/authentication") as AuthenticationSection;
				return section.Mode == AuthenticationMode.Windows;
			}
		}

		public static string ConnectionString
		{
			get { return ConfigurationManager.ConnectionStrings[RoadkillSection.Current.ConnectionStringName].ConnectionString; }
		}

		public static string RolesConnectionString
		{
			get
			{
				if (string.IsNullOrEmpty(_rolesConnectionString))
					_rolesConnectionString = GetRoleManagerConnectionString();

				return _rolesConnectionString;
			}
		}

		public static string EditorRoleName
		{
			get { return RoadkillSection.Current.EditorRoleName; }
		}

		public static string AdminRoleName
		{
			get { return RoadkillSection.Current.AdminRoleName; }
		}

		public static string AttachmentsFolder
		{
			get { return RoadkillSection.Current.AttachmentsFolder; }
		}

		public static bool Installed
		{
			get { return RoadkillSection.Current.Installed; }
		}

		public static IList<string> AllowedFileTypes
		{
			get 
			{ 
				return new List<string>(SiteConfiguration.Current.AllowedFileTypes.Split(',')); 
			}
		}

		public static string Title
		{
			get { return SiteConfiguration.Current.Title; }
		}

		public static string MarkupType
		{
			get { return SiteConfiguration.Current.MarkupType; }
		}

		public static string Theme
		{
			get { return SiteConfiguration.Current.Theme; }
		}

		public static bool CachedEnabled
		{
			get { return RoadkillSection.Current.CacheEnabled; }
		}

		public static bool CacheText
		{
			get { return RoadkillSection.Current.CacheText; }
		}

		public static string LdapConnectionString
		{
			get
			{
				if (string.IsNullOrEmpty(_ldapConnectionString))
					_ldapConnectionString = GetRoleManagerConnectionString();

				return _ldapConnectionString;
			}
		}

		public static string LdapUsername
		{
			get
			{
				if (string.IsNullOrEmpty(_ldapUsername))
					_ldapUsername = GetLdapConfigSetting("connectionUsername");

				return _ldapUsername;
			}
		}

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
		/// An asp.net relativate path e.g. ~/Themes/ to the current theme directory. Does not include a trailing slash.
		/// </summary>
		public static string ThemePath
		{
			get
			{
				return string.Format("~/Themes/{0}", Theme);
			}
		}

		public static string Version
		{
			get
			{
				return typeof(RoadkillSettings).Assembly.GetName().Version.ToString();
			}
		}

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