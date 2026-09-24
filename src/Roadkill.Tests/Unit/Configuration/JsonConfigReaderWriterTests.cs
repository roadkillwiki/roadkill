using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.Configuration
{
	[TestFixture]
	[Category("Unit")]
	public class JsonConfigReaderWriterTests
	{
		private string _configPath;

		[SetUp]
		public void Setup()
		{
			string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jsonconfigtests");
			Directory.CreateDirectory(directory);
			_configPath = Path.Combine(directory, "appsettings.json");

			if (File.Exists(_configPath))
				File.Delete(_configPath);
		}

		[Test]
		public void should_read_roadkill_section_and_connection_string()
		{
			// Arrange
			File.WriteAllText(_configPath, @"{
				// comments are allowed
				""Logging"": { ""LogLevel"": { ""Default"": ""Information"" } },
				""ConnectionStrings"": { ""Roadkill"": ""Server=.;Database=roadkill"" },
				""Roadkill"": {
					""Installed"": true,
					""DatabaseName"": ""Postgres"",
					""ApiKeys"": ""key1, key2"",
					""AttachmentsFolder"": ""~/App_Data/Files"",
					""UseBrowserCache"": true,
					""UiLanguage"": ""fr"",
					""SmtpHost"": ""smtp.example.com"",
					""SmtpPort"": 587
				}
			}");

			// Act
			var configReaderWriter = new JsonConfigReaderWriter(_configPath);
			ApplicationSettings settings = configReaderWriter.GetApplicationSettings();

			// Assert
			Assert.That(settings.Installed, Is.True);
			Assert.That(settings.ConnectionString, Is.EqualTo("Server=.;Database=roadkill"));
			Assert.That(settings.DatabaseName, Is.EqualTo("Postgres"));
			Assert.That(settings.ApiKeys, Is.EquivalentTo(new[] { "key1", "key2" }));
			Assert.That(settings.UseBrowserCache, Is.True);
			Assert.That(settings.UiLanguage, Is.EqualTo("fr"));
			Assert.That(settings.Smtp.Host, Is.EqualTo("smtp.example.com"));
			Assert.That(settings.Smtp.Port, Is.EqualTo(587));
			Assert.That(settings.AttachmentsDirectoryPath, Is.EqualTo(Path.Combine(Path.GetDirectoryName(_configPath), "App_Data", "Files") + Path.DirectorySeparatorChar));
		}

		[Test]
		[TestCase("SqlServer2008")]
		[TestCase("SqlServer2012")]
		[TestCase("SqlAzure")]
		[TestCase("")]
		public void roadkill_2_sql_server_database_names_should_be_read_as_sqlserver(string databaseName)
		{
			// Arrange
			File.WriteAllText(_configPath, @"{ ""Roadkill"": { ""Installed"": true, ""DatabaseName"": """ + databaseName + @""" } }");

			// Act
			ApplicationSettings settings = new JsonConfigReaderWriter(_configPath).GetApplicationSettings();

			// Assert
			Assert.That(settings.DatabaseName, Is.EqualTo("SqlServer"));
		}

		[Test]
		public void missing_file_should_give_uninstalled_defaults()
		{
			// Act
			var configReaderWriter = new JsonConfigReaderWriter(_configPath);
			ApplicationSettings settings = configReaderWriter.GetApplicationSettings();

			// Assert
			Assert.That(settings.Installed, Is.False);
			Assert.That(settings.DatabaseName, Is.EqualTo("SqlServer"));
			Assert.That(settings.AttachmentsRoutePath, Is.EqualTo("Attachments"));
			Assert.That(settings.IsPublicSite, Is.True);
			Assert.That(settings.UseHtmlWhiteList, Is.True);
		}

		[Test]
		public void save_should_write_section_and_connection_string_and_keep_other_settings()
		{
			// Arrange
			File.WriteAllText(_configPath, @"{ ""AllowedHosts"": ""*"", ""Roadkill"": { ""Installed"": false } }");
			var configReaderWriter = new JsonConfigReaderWriter(_configPath);

			var model = new SettingsViewModel()
			{
				ConnectionString = "Server=.;Database=roadkill",
				DatabaseName = "SqlServer",
				AdminRoleName = "Admins",
				EditorRoleName = "Editors",
				AttachmentsFolder = "~/App_Data/Attachments",
				IsPublicSite = false
			};

			// Act
			configReaderWriter.Save(model);

			// Assert
			JsonObject root = JsonNode.Parse(File.ReadAllText(_configPath)).AsObject();
			Assert.That((string)root["AllowedHosts"], Is.EqualTo("*"));
			Assert.That((string)root["ConnectionStrings"]["Roadkill"], Is.EqualTo("Server=.;Database=roadkill"));
			Assert.That((bool)root["Roadkill"]["Installed"], Is.True);
			Assert.That((string)root["Roadkill"]["AdminRoleName"], Is.EqualTo("Admins"));
			Assert.That(root["Roadkill"].AsObject().ContainsKey("ConnectionString"), Is.False, "The connection string should only be in ConnectionStrings");

			ApplicationSettings reloaded = new JsonConfigReaderWriter(_configPath).GetApplicationSettings();
			Assert.That(reloaded.Installed, Is.True);
			Assert.That(reloaded.IsPublicSite, Is.False);
			Assert.That(reloaded.ConnectionString, Is.EqualTo("Server=.;Database=roadkill"));
		}

		[Test]
		public void invalid_json_should_throw_configurationexception()
		{
			// Arrange
			File.WriteAllText(_configPath, "{ not json");

			// Act + Assert
			Assert.Throws<Roadkill.Core.ConfigurationException>(() => new JsonConfigReaderWriter(_configPath));
		}
	}
}
