using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Security;

namespace Roadkill.Core
{
	public class RoadkillSettings
	{
		public static string AdminGroup
		{
			get { return RoadkillSection.Current.AdminGroup; }
		}

		public static string AttachmentsFolder
		{
			get { return RoadkillSection.Current.AttachmentsFolder; }
		}

		public static string ConnectionString
		{
			get { return RoadkillSection.Current.ConnectionString; }
		}

		public static bool Installed
		{
			get { return RoadkillSection.Current.Installed; }
		}	

		public static string MarkupType
		{
			get { return RoadkillSection.Current.MarkupType; }
		}

		public static string Theme
		{
			get { return RoadkillSection.Current.Theme; }
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

		public static void Install(string connectionString, string adminPassword)
		{
			Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

			RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;
			section.ConnectionString = connectionString;
			section.Installed = true;

			config.Save();
		}

		public static string HashPassword(string password)
		{
			return FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");
		}
	}
}