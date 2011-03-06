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
		private static bool? _showHeader;
		private static bool? _installed;

		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["Roadkill"].ConnectionString;
			}
		}

		public static string Theme
		{
			get
			{
				return ConfigurationManager.AppSettings["Roadkill-Theme"];
			}
		}

		public static string ThemePath
		{
			get
			{
				return string.Format("~/Themes/{0}",Theme);
			}
		}

		public static bool Installed
		{
			get
			{
				if (!_installed.HasValue)
					_installed = bool.Parse(ConfigurationManager.AppSettings["Roadkill-Installed"]);

				return _installed.Value;
			}
		}

		public static string AdminGroup
		{
			get
			{
				return ConfigurationManager.AppSettings["Roadkill-AdminGroup"];
			}
		}

		public static string AttachmentsFolder
		{
			get
			{
				return ConfigurationManager.AppSettings["Roadkill-AttachmentsFolder"];
			}
		}

		public static string MarkupType
		{
			get
			{
				return ConfigurationManager.AppSettings["Roadkill-MarkupType"];
			}
		}

		public static void Install(string connectionString, string adminPassword)
		{
			Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");

			config.AppSettings.Settings["Roadkill-AdminPassword"].Value = HashPassword(adminPassword);
			config.ConnectionStrings.ConnectionStrings["Roadkill"].ConnectionString = connectionString;
			config.AppSettings.Settings["Roadkill-Installed"].Value = "true";
			config.Save();
		}

		public static string HashPassword(string password)
		{
			return FormsAuthentication.HashPasswordForStoringInConfigFile(password, "SHA1");
		}
	}
}