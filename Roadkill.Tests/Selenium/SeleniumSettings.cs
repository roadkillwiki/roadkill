using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace Roadkill.Tests.Selenium
{
	public class SeleniumSettings
	{
		public static bool UseSaucelabs
		{
			get
			{
				return Convert.ToBoolean(ConfigurationManager.AppSettings["UseSauceLabs"]);
			}
		}

		public static string BaseUrl
		{
			get
			{
				return ConfigurationManager.AppSettings["BaseUrl"];
			}
		}

		public static string SaucelabsUsername
		{
			get
			{
				return ConfigurationManager.AppSettings["Saucelabs_Username"];
			}
		}

		public static string SaucelabsAccessKey
		{
			get
			{
				return ConfigurationManager.AppSettings["Saucelabs_AccessKey"];
			}
		}

		public static string TestAdminEmail
		{
			get
			{
				return ConfigurationManager.AppSettings["TestAdmin_Email"];
			}
		}

		public static string TestAdminPassword
		{
			get
			{
				return ConfigurationManager.AppSettings["TestAdmin_Password"];
			}
		}

		public static string TestEditorEmail
		{
			get
			{
				return ConfigurationManager.AppSettings["TestEditor_Email"];
			}
		}

		public static string TestEditorPassword
		{
			get
			{
				return ConfigurationManager.AppSettings["TestEditor_Password"];
			}
		}

		public static bool HasValidSaucelabsKey
		{
			get
			{
				return !string.IsNullOrEmpty(SaucelabsAccessKey);
			}
		}

		public static string GetUrl(string path)
		{
			return new Uri(new Uri(BaseUrl), path).ToString();
		}

		public static string Dump()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine(string.Format("{0} - {1}", "UseSaucelabs", UseSaucelabs));
			builder.AppendLine(string.Format("{0} - {1}", "BaseUrl", BaseUrl));
			builder.AppendLine(string.Format("{0} - {1}", "SaucelabsUsername", SaucelabsUsername));
			builder.AppendLine(string.Format("{0} - {1}", "SaucelabsAccessKey", SaucelabsAccessKey));
			builder.AppendLine(string.Format("{0} - {1}", "TestAdminEmail", TestAdminEmail));
			builder.AppendLine(string.Format("{0} - {1}", "TestAdminPassword", TestAdminPassword));
			builder.AppendLine(string.Format("{0} - {1}", "TestEditorEmail", TestEditorEmail));
			builder.AppendLine(string.Format("{0} - {1}", "TestEditorPassword", TestEditorPassword));
			builder.AppendLine(string.Format("{0} - {1}", "TestEditorPassword", TestEditorPassword ));
			builder.AppendLine(string.Format("{0} - {1}", "HasValidSaucelabsKey", HasValidSaucelabsKey));

			return builder.ToString();
		}
	}
}
