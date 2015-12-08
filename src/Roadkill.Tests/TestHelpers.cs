using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using IisConfiguration;
using IisConfiguration.Logging;
using Microsoft.Web.Administration;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Core.Logging;
using Roadkill.Tests.Unit.StubsAndMocks;
using StructureMap;

namespace Roadkill.Tests
{
	public class TestHelpers
	{
		public static void ConfigureLocator(ApplicationSettings settings = null, bool stubRepository = true)
		{
			if (settings == null)
				settings = new ApplicationSettings();

			var configReader = new ConfigReaderWriterStub();
			configReader.ApplicationSettings = settings;

			var registry = new RoadkillRegistry(configReader);
			var container = new Container(registry);
			container.Configure(x =>
			{
				if (stubRepository)
				{
					x.Scan(a => a.AssemblyContainingType<TestHelpers>());
					x.For<IRepository>().Use(new RepositoryMock());
				}

				x.For<IUserContext>().Use(new UserContextStub());
			});

			LocatorStartup.Locator = new StructureMapServiceLocator(container, false);
			DependencyResolver.SetResolver(LocatorStartup.Locator);

			var all = container.Model.AllInstances.OrderBy(t => t.PluginType.Name).Select(t => String.Format("{0}:{1}", t.PluginType.Name, t.ReturnedType.AssemblyQualifiedName));
			Console.WriteLine(String.Join("\n", all));
		}

		public static bool IsRunningNUnitAsAdmin()
		{
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);

			return principal.IsInRole(WindowsBuiltInRole.Administrator);
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

		public static void CopyWebConfig()
		{
			try
			{
				string sitePath = TestConstants.WEB_PATH;
				string siteWebConfig = Path.Combine(sitePath, "web.config");

				string testsWebConfigPath = Path.Combine(TestConstants.LIB_FOLDER, "Configs", "web.config");
				Log.Debug("Original web.config path: {0}", siteWebConfig);
				Log.Debug("Template web.config path: {0}", testsWebConfigPath);

				// Be a good neighbour and backup the web.config
				try
				{
					string backupFile = siteWebConfig + ".bak";
					if (File.Exists(backupFile))
						File.Delete(backupFile);

					File.Copy(siteWebConfig, siteWebConfig + ".bak", true);
					Log.Debug("Backed up web.config to {0}.bak", siteWebConfig);
				}
				catch
				{
					// Ignore
				}

				File.Copy(testsWebConfigPath, siteWebConfig, true);
				Log.Debug("Copied web.config from '{0}' to '{1}'", testsWebConfigPath, siteWebConfig);
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
				string sitePath = TestConstants.WEB_PATH;
				string siteConnStringsConfig = Path.Combine(sitePath, "connectionStrings.config");

				string testsConnStringsPath = Path.Combine(TestConstants.LIB_FOLDER, "Configs", "connectionStrings.dev.config");
				Log.Debug("Original connectionStrings.config path: {0}", siteConnStringsConfig);
				Log.Debug("Acceptance tests connectionStrings.config path: {0}", testsConnStringsPath);

				// Backup
				try
				{
					string backupFile = siteConnStringsConfig + ".bak";
					if (File.Exists(backupFile))
						File.Delete(backupFile);

					File.Copy(siteConnStringsConfig, siteConnStringsConfig + ".bak", true);
					Log.Debug("Backed up connectionstrings.config to {0}.bak", siteConnStringsConfig);
				}
				catch
				{
					// Ignore the failures, it's only a connection string
				}

				File.Copy(testsConnStringsPath, siteConnStringsConfig, true);
				Log.Debug("Copied connectionstrings.config from '{0}' to '{1}'", testsConnStringsPath, siteConnStringsConfig);
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
				string sitePath = TestConstants.WEB_PATH;
				string roadkillConfig = Path.Combine(sitePath, "Roadkill.config");

				string testsRoadkillConfigPath = Path.Combine(TestConstants.LIB_FOLDER, "Configs", "Roadkill.dev.config");
				Log.Debug("Original roadkill.config path: {0}", roadkillConfig);
				Log.Debug("Acceptance tests roadkill.config path: {0}", testsRoadkillConfigPath);

				File.Copy(testsRoadkillConfigPath, roadkillConfig, true);
				Log.Debug("Copied roadkill.config from '{0}' to '{1}'", testsRoadkillConfigPath, roadkillConfig);
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
			
			if (String.IsNullOrEmpty(value))
				value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);

			if (String.IsNullOrEmpty(value))
				value = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.User);

			return value;
		}

		public class SqlServerSetup
		{
			public static void RecreateLocalDbData()
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
