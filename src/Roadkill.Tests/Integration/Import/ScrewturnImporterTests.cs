using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Import;

namespace Roadkill.Tests.Integration.Import
{
	[TestFixture]
	[Category("Integration")]
	public class ScrewturnImporterTests
	{
		private string _connectionString = TestConstants.CONNECTION_STRING;

		[SetUp]
		public void Setup()
		{
			TestHelpers.SqlServerSetup.RecreateTables();

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
		public void should_import_all_pages_categories_and_usernames()
		{
			// Arrange
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrewturnImport");

			if (Directory.Exists(applicationSettings.AttachmentsFolder))
				Directory.Delete(applicationSettings.AttachmentsFolder, true);

			Directory.CreateDirectory(applicationSettings.AttachmentsFolder);

			applicationSettings.ConnectionString = _connectionString;
			applicationSettings.DatabaseName = "SqlServer2008";

			var context = new LightSpeedContext();
			context.ConnectionString = _connectionString;
			context.DataProvider = DataProvider.SqlServer2008;
			context.IdentityMethod = IdentityMethod.GuidComb;

			IUnitOfWork unitOfWork = context.CreateUnitOfWork();

			IPageRepository pageRepository = new LightSpeedPageRepository(unitOfWork);
			IUserRepository userRepository = new LightSpeedUserRepository(unitOfWork);
			ScrewTurnImporter importer = new ScrewTurnImporter(applicationSettings, pageRepository, userRepository);

			// Act
			importer.ImportFromSqlServer(TestConstants.CONNECTION_STRING);

			// Assert
			User user = userRepository.GetUserByUsername("user2");
			Assert.That(user.Id, Is.Not.EqualTo(Guid.Empty));

			List<Page> pages = pageRepository.AllPages().ToList();
			Assert.That(pages.Count, Is.EqualTo(3));

			Page page1 = pages.FirstOrDefault(x => x.Title == "Screwturn page 1");
			PageContent pageContent1 = pageRepository.GetLatestPageContent(page1.Id);			
			Assert.That(page1.Tags, Is.EqualTo("Category1,"));

			AssertSameDateTimes(page1.CreatedOn, "2013-08-11 18:05");
			AssertSameDateTimes(page1.ModifiedOn, "2013-08-11 18:05");

			Assert.That(page1.CreatedBy, Is.EqualTo("admin"));
			Assert.That(page1.ModifiedBy, Is.EqualTo("admin"));
			Assert.That(pageContent1.Text, Is.EqualTo("This is an amazing Screwturn page."));

			Page page2 = pages.FirstOrDefault(x => x.Title == "Screwturn page 2");
			PageContent pageContent2 = pageRepository.GetLatestPageContent(page2.Id);
			Assert.That(page2.Tags, Is.EqualTo("Category1,Category2,"));

			AssertSameDateTimes(page2.CreatedOn, "2013-08-11 18:06");
			AssertSameDateTimes(page2.ModifiedOn, "2013-08-11 18:06");

			Assert.That(page2.CreatedBy, Is.EqualTo("user2"));
			Assert.That(page2.ModifiedBy, Is.EqualTo("user2"));
			Assert.That(pageContent2.Text, Is.EqualTo("Amazing screwturn page 2"));
		}

		/// <summary>
		/// TODO: fix this UTC date time issue (18:06 but the time is 19:06 in BST)
		/// </summary>
		/// <param name="date1"></param>
		/// <param name="date2Text"></param>
		private void AssertSameDateTimes(DateTime date1, string date2Text)
		{
			// Screwturn dates are assumed to be localtime of the server, and are converted to UTC by Roadkill.
			date1 = new DateTime(date1.Year, date1.Month, date1.Day, date1.Hour, date1.Minute, 0, DateTimeKind.Utc);

			// date2Text (in the SQL script) is already UTC.
			DateTime date2 = DateTime.Parse(date2Text, new CultureInfo("en-gb"), DateTimeStyles.AdjustToUniversal);
			date2 = new DateTime(date2.Year, date2.Month, date2.Day, date2.Hour, date2.Minute, 0);

			Assert.That(date1, Is.GreaterThanOrEqualTo(date2)); // hack for UTC.
		}

		[Test]
		public void should_import_files_in_attachments_folder()
		{
			// Arrange
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrewturnImport");

			if (Directory.Exists(applicationSettings.AttachmentsFolder))
				Directory.Delete(applicationSettings.AttachmentsFolder, true);

			Directory.CreateDirectory(applicationSettings.AttachmentsFolder);

			applicationSettings.ConnectionString = _connectionString;
			applicationSettings.DatabaseName = "SqlServer2008";

			var context = new LightSpeedContext();
			context.ConnectionString = _connectionString;
			context.DataProvider = DataProvider.SqlServer2008;
			context.IdentityMethod = IdentityMethod.GuidComb;

			IUnitOfWork unitOfWork = context.CreateUnitOfWork();

			IPageRepository pageRepository = new LightSpeedPageRepository(unitOfWork);
			IUserRepository userRepository = new LightSpeedUserRepository(unitOfWork);
			ScrewTurnImporter importer = new ScrewTurnImporter(applicationSettings, pageRepository, userRepository);

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
