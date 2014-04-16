using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Roadkill.Core;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Acceptance
{
	[TestFixture]
	[Category("Acceptance")]
	public class UpgradeTests : AcceptanceTestBase
	{
		private string _sqlServerMasterConnection = @"Server=(local);uid=sa;pwd=Passw0rd;database=master;";
		private string _sqlServerConnection = @"Server=(local);uid=sa;pwd=Passw0rd;database=roadkill152;";
		private string _sqliteConnection = @"Data Source=|DataDirectory|\roadkill152.sqlite;";
		private string _sqlServerCeConnection = @"Data Source=|DataDirectory|\roadkill152.sdf;";

		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			// SQL Server 1.5.2 script
			CreateSqlServer152Database();
			InstallSqlServer152Tables();

			// SQL Server CE 1.5.2. database
			string sqlServerCEDBPath = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "Upgrade", "roadkill152.sdf");
			File.Copy(sqlServerCEDBPath, Path.Combine(Settings.WEB_PATH, "App_Data", "roadkill152.sdf"), true);

			// SQLite 1.5.2 database
			string sqliteDBPath = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "Upgrade", "roadkill152.sqlite");
			string destSqlitePath = Path.Combine(Settings.WEB_PATH, "App_Data", "roadkill152.sqlite");
			File.Copy(sqliteDBPath, destSqlitePath, true);
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			// Reset the web.config back for all other acceptance tests
			ConfigFileManager.CopyConnectionStringsConfig();
		}

		[Test]
		public void SqlServer_Should_Upgrade_Then_Login_View_Existing_Page_And_Successfully_Create_New_Page()
		{
			// Arrange
			UpdateWebConfig(_sqlServerConnection, DataStoreType.SqlServer2005);
			Driver.Navigate().GoToUrl(BaseUrl);

			// Act
			Driver.FindElement(By.CssSelector("input[value=Upgrade]")).Click();
			LoginAsAdmin();
			CreatePageWithTitleAndTags("New page", "new page");
			Driver.Navigate().GoToUrl(BaseUrl);

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("This is 1.5.2 homepage"));

			Driver.Navigate().GoToUrl(BaseUrl +"/wiki/2/new-page");
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("New page"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		public void SqlServerCE_Should_Upgrade_Then_Login_View_Existing_Page_And_Successfully_Create_New_Page()
		{
			// Arrange
			UpdateWebConfig(_sqlServerCeConnection, DataStoreType.SqlServerCe);
			Driver.Navigate().GoToUrl(BaseUrl);

			// Act
			Driver.FindElement(By.CssSelector("input[value=Upgrade]")).Click();
			LoginAsAdmin();
			CreatePageWithTitleAndTags("New page", "new page");
			Driver.Navigate().GoToUrl(BaseUrl);

			// Assert
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("homepage"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("This is 1.5.2 homepage"));

			Driver.Navigate().GoToUrl(BaseUrl + "/wiki/2/new-page");
			Assert.That(Driver.FindElement(By.CssSelector(".pagetitle")).Text, Contains.Substring("New page"));
			Assert.That(Driver.FindElement(By.CssSelector("#pagecontent p")).Text, Contains.Substring("Some content goes here"));
		}

		[Test]
		public void Sqlite_Should_Show_Cannot_Upgrade_Text()
		{
			// Arrange
			UpdateWebConfig(_sqliteConnection, DataStoreType.Sqlite);
			Driver.Navigate().GoToUrl(BaseUrl);

			// Act
			string actualText = Driver.FindElement(By.CssSelector("#notsupported")).Text;
			int upgradeButtonCount = Driver.FindElements(By.CssSelector("input[value=Upgrade]")).Count;

			// Assert
			Assert.That(actualText, Contains.Substring("Automatic SQLite upgrades are not supported in Roadkill 1.6"));
			Assert.That(upgradeButtonCount, Is.EqualTo(0));
		}

		// ...2 other database tests:
		// No MySQL/Postgres upgrades as they didn't work in 1.5.2

		private void UpdateWebConfig(string connectionstring, DataStoreType databaseType)
		{
			string sitePath = Settings.WEB_PATH;
			string webConfigPath = Path.Combine(sitePath, "web.config");

			// Remove the readonly flag 
			File.SetAttributes(webConfigPath, FileAttributes.Normal);

			// Switch to previous version + update connection string in the web.config
			ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
			fileMap.ExeConfigFilename = webConfigPath;
			System.Configuration.Configuration config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
			RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;

			section.Version = "1.5.2";
			section.Installed = true;
			section.DataStoreType = databaseType.Name;
			config.ConnectionStrings.ConnectionStrings["Roadkill"].ConnectionString = connectionstring;
			config.Save(ConfigurationSaveMode.Minimal);

			Console.WriteLine("Updated {0} for upgrade tests", webConfigPath);
		}

		/// <summary>
		/// Creates a v1.5.2 database
		/// </summary>
		private void CreateSqlServer152Database()
		{
			string sql = @"USE [master];
						IF  EXISTS (SELECT name FROM sys.databases WHERE name = N'Roadkill152')
						BEGIN
							DROP DATABASE [Roadkill152]
						END;

						CREATE DATABASE [Roadkill152];";

			using (SqlConnection connection = new SqlConnection(_sqlServerMasterConnection))
			{
				SqlConnection.ClearAllPools();
				connection.Open();

				SqlCommand command = connection.CreateCommand();
				command.CommandText = sql;
				command.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// The tables and data.
		/// </summary>
		private void InstallSqlServer152Tables()
		{
			string scriptPath = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "Upgrade", "roadkill152.sqlserver.sql");
			string sql = File.ReadAllText(scriptPath);

			string[] sqlCommands = sql.Split(';');

			using (SqlConnection connection = new SqlConnection(_sqlServerConnection))
			{
				SqlConnection.ClearAllPools();
				connection.Open();

				SqlCommand command = connection.CreateCommand();

				foreach (string statement in sqlCommands)
				{
					if (!string.IsNullOrEmpty(statement))
					{
						command.CommandText = statement;
						command.ExecuteNonQuery();
					}
				}
			}
		}
	}
}
