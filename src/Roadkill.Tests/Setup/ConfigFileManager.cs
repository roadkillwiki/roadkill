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
		public static void CopyWebConfig()
		{
			try
			{
				string sitePath = Settings.WEB_PATH;
				string siteWebConfig = Path.Combine(sitePath, "web.config");

				string testsWebConfigPath = Path.Combine(Settings.LIB_FOLDER, "Configs", "web.config");
				Console.WriteLine("Original web.config path: {0}", siteWebConfig);
				Console.WriteLine("Template web.config path: {0}", testsWebConfigPath);

				// Be a good neighbour and backup the web.config
				try
				{
					string backupFile = siteWebConfig + ".bak";
					if (File.Exists(backupFile))
						File.Delete(backupFile);

					File.Copy(siteWebConfig, siteWebConfig + ".bak", true);
					Console.WriteLine("Backed up web.config to {0}.bak", siteWebConfig);
				}
				catch
				{
					// Ignore
				}

				File.Copy(testsWebConfigPath, siteWebConfig, true);
				Console.WriteLine("Copied web.config from '{0}' to '{1}'", testsWebConfigPath, siteWebConfig);
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
				Console.WriteLine("Original connectionStrings.config path: {0}", siteConnStringsConfig);
				Console.WriteLine("Acceptance tests connectionStrings.config path: {0}", testsConnStringsPath);

				// Backup
				try
				{
					string backupFile = siteConnStringsConfig + ".bak";
					if (File.Exists(backupFile))
						File.Delete(backupFile);

					File.Copy(siteConnStringsConfig, siteConnStringsConfig + ".bak", true);
					Console.WriteLine("Backed up connectionstrings.config to {0}.bak", siteConnStringsConfig);
				}
				catch
				{
					// Ignore the failures, it's only a connection string
				}

				File.Copy(testsConnStringsPath, siteConnStringsConfig, true);
				Console.WriteLine("Copied connectionstrings.config from '{0}' to '{1}'", testsConnStringsPath, siteConnStringsConfig);
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
				Console.WriteLine("Original roadkill.config path: {0}", roadkillConfig);
				Console.WriteLine("Acceptance tests roadkill.config path: {0}", testsRoadkillConfigPath);

				File.Copy(testsRoadkillConfigPath, roadkillConfig, true);
				Console.WriteLine("Copied roadkill.config from '{0}' to '{1}'", testsRoadkillConfigPath, roadkillConfig);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}
