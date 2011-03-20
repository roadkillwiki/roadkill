using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;
using System.Web.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Both application and web.config settings for the Roadkill instance.
	/// </summary>
	public class RoadkillSettings
	{
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

		/// <summary>
		/// An asp.net relativate path e.g. ~/Themes/ to the current theme directory. Does not include
		/// a trailing slash.
		/// </summary>
		public static string ThemePath
		{
			get
			{
				return string.Format("~/Themes/{0}", Theme);
			}
		}
	}
}