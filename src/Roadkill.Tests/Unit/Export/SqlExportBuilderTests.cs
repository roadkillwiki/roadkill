using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Export;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SqlExportBuilderTests
	{
		// These are not integration tests - they just compare the SQL exported to a 'good' sql text.
		// Some full integration tests for each database type can be added if needed later.

		[Test]
		public void Should_Export_Users_With_All_FieldValues()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings.PluginLastSaveDate = DateTime.Today;

			Guid user1Id = new Guid("29a8ad19-b203-46f5-be10-11e0ebf6f812");
			Guid user2Id = new Guid("e63b0023-329a-49b9-97a4-5094a0e378a2");
			Guid user3Id = new Guid("a6ee19ef-c093-47de-97d2-83dec406d92d");

			Guid user1Activationkey = new Guid("0953cf95-f357-4e5b-ae2b-7541844d3b6b");
			Guid user2Activationkey = new Guid("aa87fe31-9781-4c93-b7e3-9092ed095810");
			Guid user3Activationkey = new Guid("b8ef994d-87f5-4543-85de-66b41244a20a");

			User user1 = new User()
			{
				Id = user1Id,
				ActivationKey = user1Activationkey.ToString(),
				Firstname = "firstname1",
				Lastname = "lastname1",
				Email = "user1@localhost", 
				Password = "encrypted1",
				Salt = "salt1",
				IsActivated = true,
				IsAdmin = true,
				Username = "user1"
			};

			User user2 = new User()
			{
				Id = user2Id,
				ActivationKey = user2Activationkey.ToString(),
				Firstname = "firstname2",
				Lastname = "lastname2",
				Email = "user2@localhost",
				Password = "encrypted2",
				Salt = "salt2",
				IsActivated = true,
				IsEditor = true,
				Username = "user2"
			};

			User user3 = new User()
			{
				Id = user3Id,
				ActivationKey = user3Activationkey.ToString(),
				Firstname = "firstname3",
				Lastname = "lastname3",
				Email = "user3@localhost",
				Password = "encrypted3",
				Salt = "salt3",
				IsActivated = false,
				IsEditor = true,
				Username = "user3"
			};

			repository.Users.Add(user1);
			repository.Users.Add(user2);
			repository.Users.Add(user3);

			SqlExportBuilder builder = new SqlExportBuilder(repository, new PluginFactoryMock());
			builder.IncludeConfiguration = false;
			builder.IncludePages = false;
			string expectedSql = ReadEmbeddedResource("expected-users-export.sql");

			// Act
			string actualSql = builder.Export();

			// Assert
			Assert.That(actualSql, Is.EqualTo(expectedSql), actualSql);
		}

		[Test]
		public void Should_Export_Pages_With_Content()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings.PluginLastSaveDate = DateTime.Today;

			DateTime page1CreatedOn  = new DateTime(2013, 01, 01, 12, 00, 00);
			DateTime page1ModifiedOn = new DateTime(2013, 01, 01, 13, 00, 00);
			DateTime page2CreatedOn  = new DateTime(2013, 01, 02, 12, 00, 00);
			DateTime page2ModifiedOn = new DateTime(2013, 01, 02, 13, 00, 00);
			DateTime page3CreatedOn  = new DateTime(2013, 01, 03, 12, 00, 00);
			DateTime page3ModifiedOn = new DateTime(2013, 01, 03, 13, 00, 00);

			Guid page1ContentId = new Guid("13a8ad19-b203-46f5-be10-11e0ebf6f812");
			Guid page2ContentId = new Guid("143b0023-329a-49b9-97a4-5094a0e378a2");
			Guid page3ContentId = new Guid("15ee19ef-c093-47de-97d2-83dec406d92d");

			string page1Text = @"the text ;'''


								"" more text """;

			string page2Text = @"the text ;''' #### sdfsdfsdf ####


								"" blah text """;

			string page3Text = @"the text ;''' #### dddd **dddd** ####			
			

								"" pppp text """;

			Page page1 = new Page()
			{
				CreatedBy = "created-by-user1",
				CreatedOn = page1CreatedOn,
				Id = 1,
				IsLocked = true,
				ModifiedBy = "modified-by-user2",
				ModifiedOn = page1ModifiedOn,
				Tags = "tag1,tag2,tag3",
				Title = "Page 1 title"
			};

			Page page2 = new Page()
			{
				CreatedBy = "created-by-user2",
				CreatedOn = page2CreatedOn,
				Id = 2,
				IsLocked = true,
				ModifiedBy = "modified-by-user2",
				ModifiedOn = page2ModifiedOn,
				Tags = "tagA,tagB,tagC",
				Title = "Page 2 title"
			};

			Page page3 = new Page()
			{
				CreatedBy = "created-by-user3",
				CreatedOn = page3CreatedOn,
				Id = 3,
				IsLocked = false,
				ModifiedBy = "modified-by-user3",
				ModifiedOn = page3ModifiedOn,
				Tags = "tagX,tagY,tagZ",
				Title = "Page 3 title"
			};

			PageContent pageContent1 = repository.AddNewPage(page1, page1Text, "modified-by-user1", page1ModifiedOn);
			pageContent1.Id = page1ContentId;

			PageContent pageContent2 = repository.AddNewPage(page2, page2Text, "modified-by-user2", page2ModifiedOn);
			pageContent2.Id = page2ContentId;

			PageContent pageContent3 = repository.AddNewPage(page3, page3Text, "modified-by-user3", page3ModifiedOn);
			pageContent3.Id = page3ContentId;

			SqlExportBuilder builder = new SqlExportBuilder(repository, new PluginFactoryMock());
			builder.IncludeConfiguration = false;
			builder.IncludePages = true;

			string expectedSql = ReadEmbeddedResource("expected-pages-export.sql");

			// Act
			string actualSql = builder.Export();

			// Assert
			Assert.That(actualSql, Is.EqualTo(expectedSql), actualSql);
		}

		[Test]
		public void Should_Export_SiteConfiguration_And_Plugin_Settings()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings.PluginLastSaveDate = new DateTime(2013, 11, 09, 0, 0, 0);
			repository.SiteSettings.AllowedFileTypes = ".exe,.vbscript";
			repository.SiteSettings.MenuMarkup = "markup ```''' \r\n";
			
			// Plugins setup
			SiteCache siteCache = new SiteCache(new ApplicationSettings(), new CacheMock());

			TextPluginStub plugin1 = new TextPluginStub("fake-plugin1", "fake plugin1", "description 1", "1.1");
			plugin1.PluginCache = siteCache;
			plugin1.Repository = repository;
			plugin1.Settings.IsEnabled = true;
			plugin1.Settings.SetValue("key1", "value1");
			plugin1.Settings.SetValue("key2", "value2");

			TextPluginStub plugin2 = new TextPluginStub("fake-plugin2", "fake plugin2", "description 2", "2.1");
			plugin2.PluginCache = siteCache;
			plugin2.Repository = repository;

			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			pluginFactory.TextPlugins.Add(plugin1);
			pluginFactory.TextPlugins.Add(plugin2);

			// SqlExportBuilder
			SqlExportBuilder builder = new SqlExportBuilder(repository, pluginFactory);
			builder.IncludeConfiguration = true;
			builder.IncludePages = false;

			string expectedSql = ReadEmbeddedResource("expected-siteconfiguration-export.sql");
			expectedSql = expectedSql.Replace("{AppVersion}", ApplicationSettings.ProductVersion);

			// Act
			string actualSql = builder.Export();

			// Assert
			Assert.That(actualSql, Is.EqualTo(expectedSql), actualSql);
		}

		private string ReadEmbeddedResource(string name)
		{
			// These files need to have Windows line spacing (\r\n). To convert the files use Notepad++ - Edit->EOL Conversion
			string path = string.Format("Roadkill.Tests.Unit.Export.{0}", name);

			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
			if (stream == null)
				throw new InvalidOperationException(string.Format("Unable to find '{0}' as an embedded resource", path));

			string result = "";
			using (StreamReader reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}

			return result;
		}
	}
}
