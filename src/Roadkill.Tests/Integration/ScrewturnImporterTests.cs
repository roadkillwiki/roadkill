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
	[Explicit("Required localdb (SQL Server 2012)")]
	public class ScrewturnImporterTests
	{
		private string _connectionString = @"Server=(LocalDB)\v11.0;Integrated Security=true;";

		[SetUp]
		public void Setup()
		{
			string sqlFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration", "screwturn3.sql");
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
		public void Should_Import_All_Pages_And_Categories_And_Usernames()
		{
			// Arrange
			ApplicationSettings applicationSettings = new ApplicationSettings();
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
			List<Page> pages = repository.AllPages().ToList();
			Assert.That(pages.Count, Is.EqualTo(3));

			User user = repository.GetUserByUsername("user2");
			Assert.That(user.Id, Is.Not.EqualTo(Guid.Empty)); 

			Page page1 = pages.FirstOrDefault(x => x.Title == "Screwturn page 1");
			Page page2 = pages.FirstOrDefault(x => x.Title == "Screwturn page 2");

			Assert.That(page1.Tags, Is.EqualTo("Category1;"));
			Assert.That(page2.Tags, Is.EqualTo("Category1;Category2;"));

			PageContent pageContent1 = repository.GetLatestPageContent(page1.Id);
			PageContent pageContent2 = repository.GetLatestPageContent(page2.Id);

			Assert.That(pageContent1.Text, Is.EqualTo("This is an amazing Screwturn page."));
			Assert.That(pageContent2.Text, Is.EqualTo("Amazing screwturn page 2"));
		}
	}
}
