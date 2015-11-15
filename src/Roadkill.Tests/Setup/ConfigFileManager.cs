using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Tests
{
	public class ConfigFileManager
	{
		private static bool _logEnabled = false;

		public void EnableLogging()
		{
			_logEnabled = true;
		}

		public static void Log(string text, params object[] args)
		{
			if (_logEnabled)
			{
				Console.WriteLine(text, args);
			}
		}

		public static void CopyWebConfig()
		{
			try
			{
				string sitePath = Settings.WEB_PATH;
				string siteWebConfig = Path.Combine(sitePath, "web.config");

				string testsWebConfigPath = Path.Combine(Settings.LIB_FOLDER, "Configs", "web.config");
				Log("Original web.config path: {0}", siteWebConfig);
				Log("Template web.config path: {0}", testsWebConfigPath);

				// Be a good neighbour and backup the web.config
				try
				{
					string backupFile = siteWebConfig + ".bak";
					if (File.Exists(backupFile))
						File.Delete(backupFile);

					File.Copy(siteWebConfig, siteWebConfig + ".bak", true);
					Log("Backed up web.config to {0}.bak", siteWebConfig);
				}
				catch
				{
					// Ignore
				}

				File.Copy(testsWebConfigPath, siteWebConfig, true);
				Log("Copied web.config from '{0}' to '{1}'", testsWebConfigPath, siteWebConfig);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public static void CopyConnectionStringsConfig()
		{
			try
			{
				string sitePath = Settings.WEB_PATH;
				string siteConnStringsConfig = Path.Combine(sitePath, "connectionStrings.config");

				string testsConnStringsPath = Path.Combine(Settings.LIB_FOLDER, "Configs", "connectionStrings.dev.config");
				Log("Original connectionStrings.config path: {0}", siteConnStringsConfig);
				Log("Acceptance tests connectionStrings.config path: {0}", testsConnStringsPath);

				// Backup
				try
				{
					string backupFile = siteConnStringsConfig + ".bak";
					if (File.Exists(backupFile))
						File.Delete(backupFile);

					File.Copy(siteConnStringsConfig, siteConnStringsConfig + ".bak", true);
					Log("Backed up connectionstrings.config to {0}.bak", siteConnStringsConfig);
				}
				catch
				{
					// Ignore the failures, it's only a connection string
				}

				File.Copy(testsConnStringsPath, siteConnStringsConfig, true);
				Log("Copied connectionstrings.config from '{0}' to '{1}'", testsConnStringsPath, siteConnStringsConfig);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public static void CopyRoadkillConfig()
		{
			try
			{
				string sitePath = Settings.WEB_PATH;
				string roadkillConfig = Path.Combine(sitePath, "Roadkill.config");

				string testsRoadkillConfigPath = Path.Combine(Settings.LIB_FOLDER, "Configs", "Roadkill.dev.config");
				Log("Original roadkill.config path: {0}", roadkillConfig);
				Log("Acceptance tests roadkill.config path: {0}", testsRoadkillConfigPath);

				File.Copy(testsRoadkillConfigPath, roadkillConfig, true);
				Log("Copied roadkill.config from '{0}' to '{1}'", testsRoadkillConfigPath, roadkillConfig);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
