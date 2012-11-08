using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Domain;
using Roadkill.Core.Search;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	public class SearchManagerTests : MoqTestBase
	{
		[SetUp]
		public void SearchSetup()
		{
			// Required by the indexer to parser the markup
			ServiceContainer.Current.Configuration.MarkupType = "Creole";

			string indexPath = AppDomain.CurrentDomain.BaseDirectory + "App_Data/search";
			if (Directory.Exists(indexPath))
				Directory.Delete(indexPath, true);
		}

		[Test]
		public void Search_With_No_Field_Returns_Results()
		{
			// Arrange
			SearchManager searchManager = new SearchManager();
			searchManager.CreateIndex();
			searchManager.Add(AddMockPage(1, "admin", "random name 1", "tag1", "title content 1").ToSummary());
			searchManager.Add(AddMockPage(2, "admin", "random name 2", "tag1", "title content 2").ToSummary());

			// Act
			List<SearchResult> results = searchManager.SearchIndex("title content").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(2)); // Lucene will ignore the 1 and 2 in the content
		}

		[Test]
		public void Search_By_Title()
		{
			// Arrange
			SearchManager searchManager = new SearchManager();
			searchManager.CreateIndex();
			searchManager.Add(AddMockPage(1, "admin", "the title", "tag1", "title content").ToSummary());
			searchManager.Add(AddMockPage(2, "admin", "random name 1", "tag1", "title content").ToSummary());
			searchManager.Add(AddMockPage(3, "admin", "random name 2", "tag1", "title content").ToSummary());
			searchManager.Add(AddMockPage(4, "admin", "random name 3", "tag1", "title content").ToSummary());

			// Act
			List<SearchResult> results = searchManager.SearchIndex("title:\"the title\"").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(1));
		}

		[Test]
		public void Search_By_TagsField_Returns_Multiple_Results()
		{
			// Arrange
			SearchManager searchManager = new SearchManager();
			searchManager.CreateIndex();
			searchManager.Add(AddMockPage(1, "admin", "random name 1", "homepage, tag1", "title content 1").ToSummary());
			searchManager.Add(AddMockPage(2, "admin", "random name 2", "tag1, tag", "title content 2").ToSummary());
			searchManager.Add(AddMockPage(3, "admin", "random name 3", "tag3, tag", "title content 3").ToSummary());
			searchManager.Add(AddMockPage(4, "admin", "random name 4", "tag4, tag", "title content 4").ToSummary());

			// Act
			List<SearchResult> results = searchManager.SearchIndex("tags:\"tag1\"").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(2));
		}

		[Test]
		public void Search_By_IdField_Returns_Single_Results()
		{
			// Arrange
			SearchManager searchManager = new SearchManager();
			searchManager.CreateIndex();
			searchManager.Add(AddMockPage(1, "admin", "random name 1", "tag").ToSummary());
			searchManager.Add(AddMockPage(2, "admin", "random name 2", "1tag").ToSummary());
			searchManager.Add(AddMockPage(3, "admin", "random name 3", "1tag").ToSummary());
			searchManager.Add(AddMockPage(4, "admin", "random name 4", "1tag").ToSummary());

			// Act
			List<SearchResult> results = searchManager.SearchIndex("id:1").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(1));
		}

		[Test]
		public void CreatedBy_Only_Searchable_Using_Field_Syntax()
		{
			// Arrange
			SearchManager searchManager = new SearchManager();
			searchManager.CreateIndex();
			searchManager.Add(AddMockPage(1, "admin", "random name 1", "homepage, tag1", "title content 1").ToSummary());
			searchManager.Add(AddMockPage(2, "admin", "random name 2", "tag1, tag", "title content 2").ToSummary());

			// Act
			List<SearchResult> results = searchManager.SearchIndex("admin").ToList();
			List<SearchResult> createdByResults = searchManager.SearchIndex("createdby: admin").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(0), "admin title count");
			Assert.That(createdByResults.Count, Is.EqualTo(2), "createdby count");
		}

		[Test]
		public void Search_By_CreatedOnField_Returns_Results()
		{
			// Arrange
			string todaysDate = DateTime.Today.ToShortDateString(); // (CreateMockPage uses Today date, SearchManager uses 's'/ISO8601)
			SearchManager searchManager = new SearchManager();
			searchManager.CreateIndex();
			searchManager.Add(AddMockPage(1, "admin", "random name 1", "homepage, tag1", DateTime.Today).ToSummary());
			searchManager.Add(AddMockPage(2, "admin", "random name 2", "tag1, tag", DateTime.Today).ToSummary());
			searchManager.Add(AddMockPage(3, "admin", "random name 3", "tag1, tag", DateTime.Today.AddDays(1)).ToSummary());
			searchManager.Add(AddMockPage(4, "admin", "random name 4", "tag1, tag", DateTime.Today.AddDays(2)).ToSummary());

			// Act
			List<SearchResult> createdOnResults = searchManager.SearchIndex("createdon:" +todaysDate).ToList();

			// Assert
			Assert.That(createdOnResults.Count, Is.EqualTo(2), "createdon count");
		}

		[Test]
		public void Delete_Should_Remove_Page_From_Index()
		{
			// Arrange
			SearchManager searchManager = new SearchManager();
			searchManager.CreateIndex();
			PageSummary summary1 = AddMockPage(1, "admin", "homepage title", "homepage, tag1", DateTime.Today).ToSummary();
			PageSummary summary2 = AddMockPage(2, "admin", "random name 2", "tag1, tag", DateTime.Today).ToSummary();
			searchManager.Add(summary1);
			searchManager.Add(summary2);

			// Act
			searchManager.Delete(summary1);
			List<SearchResult> results = searchManager.SearchIndex("homepage title").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(0), "homepage title still appears after deletion");
		}

		[Test]
		public void Update_Should_Show_In_Index_Search()
		{
			// Arrange
			SearchManager searchManager = new SearchManager();
			searchManager.CreateIndex();
			PageSummary summary1 = AddMockPage(1, "admin", "homepage title", "homepage, tag1", DateTime.Today).ToSummary();
			PageSummary summary2 = AddMockPage(2, "admin", "random name 2", "tag1, tag", DateTime.Today).ToSummary();
			searchManager.Add(summary1);
			searchManager.Add(summary2);

			// Act
			summary1.Title = "A new title";
			searchManager.Update(summary1);

			Thread thread = new Thread(delegate()
			{
				// Perform the test in a new thread, so that the add + delete commit is picked up
				// which is periodically done by Lucene.
				List<SearchResult> oldResults = searchManager.SearchIndex("homepage title").ToList();
				List<SearchResult> newResults = searchManager.SearchIndex("A new title").ToList();

				// Assert
				Assert.That(oldResults.Count, Is.EqualTo(0), "old results");
				Assert.That(newResults.Count, Is.EqualTo(1), "new results");
			});
			thread.Start();
		}
	}
}
