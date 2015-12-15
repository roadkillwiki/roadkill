using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using IisConfiguration;
using IisConfiguration.Logging;
using Microsoft.Web.Administration;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Tests.Unit.StubsAndMocks;
using StructureMap;
using Configuration = System.Configuration.Configuration;

namespace Roadkill.Tests
{
	public class TestHelpers
	{
		public static bool IsSqlServerRunning()
		{
			return Process.GetProcessesByName("sqlservr").Any();
		}

		public static bool IsMongoDBRunning()
		{
			return Process.GetProcessesByName("mongod").Any();
		}

		public static void CreateIisTestSite()
		{
			var logger = new ConsoleLogger();
			var serverConfig = new WebServerConfig(logger);

			// Current directory: src\Roadkill.Tests\bin\Debug
			string webRoot = Environment.CurrentDirectory + @"..\..\..\..\Roadkill.Web";
			var dirInfo = new DirectoryInfo(webRoot);

			serverConfig
				.AddAppPool(TestConstants.WEB_SITENAME, "v4.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalService)
				.WithProcessModel(TimeSpan.FromMinutes(60), false)
				.Commit();

			serverConfig
				.AddSite(TestConstants.WEB_SITENAME, TestConstants.WEB_PORT, TestConstants.WEB_PORT)
				.AddApplication("/", dirInfo.FullName, TestConstants.WEB_SITENAME)
				.WithLogging(false)
				.Commit();
		}

		public static void SetRoadkillConfigToUnInstalled()
		{
			string sitePath = TestConstants.WEB_PATH;
			string webConfigPath = Path.Combine(sitePath, "web.config");
			string roadkillConfigPath = Path.Combine(sitePath, "roadkill.config");

			// Remove the readonly flag from one of the installer tests (this could be fired in any order)
			File.SetAttributes(webConfigPath, FileAttributes.Normal);
			File.SetAttributes(roadkillConfigPath, FileAttributes.Normal);

			// Switch installed=false in the web.config (roadkill.config)
			ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
			fileMap.ExeConfigFilename = webConfigPath;
			Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
			RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;

			section.Installed = false;
			config.ConnectionStrings.ConnectionStrings["Roadkill"].ConnectionString = "";
			config.Save(ConfigurationSaveMode.Minimal);
		}

		public static void DeleteAttachmentsFolder()
		{
			try
			{
				// Remove any attachment folders used by the installer tests
				string installerTestsAttachmentsPath = Path.Combine(TestConstants.WEB_PATH, "AcceptanceTests");
				Directory.Delete(installerTestsAttachmentsPath, true);
			}
			catch { }
		}

		public static void CopyDevWebConfigFromLibFolder()
		{
			try
			{
				string sitePath = TestConstants.WEB_PATH;
				string siteWebConfig = Path.Combine(sitePath, "web.config");

				string testsWebConfigPath = Path.Combine(TestConstants.LIB_FOLDER, "Configs", "web.config");

				// Be a good neighbour and backup the web.config
				try
				{
					string backupFile = siteWebConfig + ".bak";
					if (File.Exists(backupFile))
						File.Delete(backupFile);

					File.Copy(siteWebConfig, siteWebConfig + ".bak", true);
				}
				catch
				{
					// Ignore
				}

				File.Copy(testsWebConfigPath, siteWebConfig, true);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public static void CopyDevConnectionStringsConfig()
		{
			try
			{
				string sitePath = TestConstants.WEB_PATH;
				string siteConnStringsConfig = Path.Combine(sitePath, "connectionStrings.config");

				string testsConnStringsPath = Path.Combine(TestConstants.LIB_FOLDER, "Configs", "connectionStrings.dev.config");

				// Backup
				try
				{
					string backupFile = siteConnStringsConfig + ".bak";
					if (File.Exists(backupFile))
						File.Delete(backupFile);

					File.Copy(siteConnStringsConfig, siteConnStringsConfig + ".bak", true);
				}
				catch
				{
					// Ignore the failures, it's only a connection string
				}

				File.Copy(testsConnStringsPath, siteConnStringsConfig, true);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public static void CopyDevRoadkillConfig()
		{
			try
			{
				string sitePath = TestConstants.WEB_PATH;
				string roadkillConfig = Path.Combine(sitePath, "Roadkill.config");

				string testsRoadkillConfigPath = Path.Combine(TestConstants.LIB_FOLDER, "Configs", "Roadkill.dev.config");

				File.Copy(testsRoadkillConfigPath, roadkillConfig, true);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		public static string GetEnvironmentalVariable(string name)
		{
			// Tries to gets an environmental variable from any of the 3 env sources.
			string value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Machine);
			
			if (string.IsNullOrEmpty(value))
				value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

			if (string.IsNullOrEmpty(value))
				value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);

			return value;
		}

		public class SqlServerSetup
		{
			public static void RecreateTables()
			{
				using (SqlConnection connection = new SqlConnection(TestConstants.CONNECTION_STRING))
				{
					connection.Open();

					SqlCommand command = connection.CreateCommand();
					command.CommandText = ReadSqlServerScript();

					command.ExecuteNonQuery();
				}
			}

			public static void ClearDatabase()
			{
				using (SqlConnection connection = new SqlConnection(TestConstants.CONNECTION_STRING))
				{
					connection.Open();

					SqlCommand command = connection.CreateCommand();
					command.CommandText =
						"DELETE FROM roadkill_pagecontent;" +
						"DELETE FROM roadkill_pages;" +
						"DELETE FROM roadkill_users;" +
						"DELETE FROM roadkill_siteconfiguration;" +
						"DBCC CHECKIDENT (roadkill_pages, RESEED, 1);";

					command.ExecuteNonQuery();
				}
			}

			private static string ReadSqlServerScript()
			{
				string path = Path.Combine(TestConstants.LIB_FOLDER, "Test-databases", "roadkill-sqlserver.sql");
				return File.ReadAllText(path);
			}
		}
	}
}
