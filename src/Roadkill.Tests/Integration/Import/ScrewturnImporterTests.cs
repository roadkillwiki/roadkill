using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Import;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	[Category("Integration")]
	public class ScrewturnImporterTests
	{
		private string _connectionString = SqlExpressSetup.ConnectionString;

		[SetUp]
		public void Setup()
		{
			SqlExpressSetup.RecreateLocalDbData();

			string sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration", "Import", "screwturn3.sql");
			string sqlCommands = File.ReadAllText(sqlFile);

			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandText = sqlCommands;
					command.ExecuteNonQuery();
				}
			}
		}

		[Test]
		public void Should_Import_All_Pages_Categories_And_Usernames()
		{
			// Arrange
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrewturnImport");

			if (Directory.Exists(applicationSettings.AttachmentsFolder))
				Directory.Delete(applicationSettings.AttachmentsFolder, true);

			Directory.CreateDirectory(applicationSettings.AttachmentsFolder);

			applicationSettings.ConnectionString = _connectionString;
			applicationSettings.DataStoreType = DataStoreType.SqlServer2012;

			IRepository repository = new LightSpeedRepository(applicationSettings);
			repository.Startup(applicationSettings.DataStoreType,
								applicationSettings.ConnectionString,
								false);

			// Clear the database
			repository.Install(applicationSettings.DataStoreType,
								applicationSettings.ConnectionString,
								false);
			ScrewTurnImporter importer = new ScrewTurnImporter(applicationSettings, repository);

			// Act
			importer.ImportFromSqlServer(SqlExpressSetup.ConnectionString);

			// Assert
			User user = repository.GetUserByUsername("user2");
			Assert.That(user.Id, Is.Not.EqualTo(Guid.Empty));

			List<Page> pages = repository.AllPages().ToList();
			Assert.That(pages.Count, Is.EqualTo(3));

			Page page1 = pages.FirstOrDefault(x => x.Title == "Screwturn page 1");
			PageContent pageContent1 = repository.GetLatestPageContent(page1.Id);			
			Assert.That(page1.Tags, Is.EqualTo("Category1;"));
			Assert.That(page1.CreatedOn.ToString("u"), Is.EqualTo("2013-08-11 19:05:49Z"));
			Assert.That(page1.ModifiedOn.ToString("u"), Is.EqualTo("2013-08-11 19:05:49Z"));
			Assert.That(page1.CreatedBy, Is.EqualTo("admin"));
			Assert.That(page1.ModifiedBy, Is.EqualTo("admin"));
			Assert.That(pageContent1.Text, Is.EqualTo("This is an amazing Screwturn page."));

			Page page2 = pages.FirstOrDefault(x => x.Title == "Screwturn page 2");
			PageContent pageContent2 = repository.GetLatestPageContent(page2.Id);
			Assert.That(page2.Tags, Is.EqualTo("Category1;Category2;"));
			Assert.That(page2.CreatedOn.ToString("u"), Is.EqualTo("2013-08-11 19:06:54Z"));
			Assert.That(page2.ModifiedOn.ToString("u"), Is.EqualTo("2013-08-11 19:06:54Z"));
			Assert.That(page2.CreatedBy, Is.EqualTo("user2"));
			Assert.That(page2.ModifiedBy, Is.EqualTo("user2"));
			Assert.That(pageContent2.Text, Is.EqualTo("Amazing screwturn page 2"));
		}

		[Test]
		public void Should_Import_Files_In_Attachments_Folder()
		{
			// Arrange
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrewturnImport");

			if (Directory.Exists(applicationSettings.AttachmentsFolder))
				Directory.Delete(applicationSettings.AttachmentsFolder, true);

			Directory.CreateDirectory(applicationSettings.AttachmentsFolder);

			applicationSettings.ConnectionString = _connectionString;
			applicationSettings.DataStoreType = DataStoreType.SqlServer2012;

			IRepository repository = new LightSpeedRepository(applicationSettings);
			repository.Startup(applicationSettings.DataStoreType,
								applicationSettings.ConnectionString,
								false);

			// Clear the database
			repository.Install(applicationSettings.DataStoreType,
								applicationSettings.ConnectionString,
								false);
			ScrewTurnImporter importer = new ScrewTurnImporter(applicationSettings, repository);

			// Act
			importer.ImportFromSqlServer(_connectionString);

			// Assert
			string file1 = Path.Combine(applicationSettings.AttachmentsFolder, "atextfile.txt");
			string file2 = Path.Combine(applicationSettings.AttachmentsFolder, "screwdriver1.jpg");
			FileInfo fileInfo1 = new FileInfo(file1);
			FileInfo fileInfo2 = new FileInfo(file2);

			Assert.True(fileInfo1.Exists);
			Assert.True(fileInfo2.Exists);

			Assert.That(fileInfo1.Length, Is.GreaterThan(0));
			Assert.That(fileInfo2.Length, Is.GreaterThan(0));
		}
	}
}
