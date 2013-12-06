using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Services;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	[Category("Integration")]
	public class SearchServiceTests
	{
		private IRepository _repository;
		private ApplicationSettings _config;
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Initialize()
		{
			string indexPath = AppDomain.CurrentDomain.BaseDirectory + @"\App_Data\SearchTests";
			if (Directory.Exists(indexPath))
				Directory.Delete(indexPath, true);

			_repository = new RepositoryMock();
			_config = new ApplicationSettings();
			_config.Installed = true;
			_pluginFactory = new PluginFactoryMock();
		}

		[Test]
		public void Search_With_No_Field_Returns_Results()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "title content", "tag1", "title content1");
			PageViewModel page2 = CreatePage(2, "admin", "title content", "tag1", "title content2");

			searchService.Add(page1);
			searchService.Add(page2);

			// Act
			List<SearchResultViewModel> results = searchService.Search("title content").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(2)); // Lucene will ignore the 1 and 2 in the content
		}

		[Test]
		public void Search_By_Title()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "the title", "tag1", "title content");
			PageViewModel page2 = CreatePage(2, "admin", "random name1", "tag1", "title content");
			PageViewModel page3 = CreatePage(3, "admin", "random name2", "tag1", "title content");
			PageViewModel page4 = CreatePage(4, "admin", "random name3", "tag1", "title content");

			searchService.Add(page1);
			searchService.Add(page2);
			searchService.Add(page3);
			searchService.Add(page4);

			// Act
			List<SearchResultViewModel> results = searchService.Search("title:\"the title\"").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(1));
		}

		[Test]
		public void Search_By_TagsField_Returns_Multiple_Results()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "random name1", "homepage1, tag1", "title content");
			PageViewModel page2 = CreatePage(2, "admin", "random name2", "tag1, tag", "title content");
			PageViewModel page3 = CreatePage(3, "admin", "random name3", "tag3, tag", "title content");
			PageViewModel page4 = CreatePage(4, "admin", "random name4", "tag4, tag", "title content");

			searchService.Add(page1);
			searchService.Add(page2);
			searchService.Add(page3);
			searchService.Add(page4);

			// Act
			List<SearchResultViewModel> results = searchService.Search("tags:\"tag1\"").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(2));
		}

		[Test]
		public void Search_By_IdField_Returns_Single_Results()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "random name2", "tag1", "title content");
			PageViewModel page2 = CreatePage(2, "admin", "random name2", "1tag1", "title content");
			PageViewModel page3 = CreatePage(3, "admin", "random name3", "1tag1", "title content");
			PageViewModel page4 = CreatePage(4, "admin", "random name4", "1tag1", "title content");

			searchService.Add(page1);
			searchService.Add(page2);
			searchService.Add(page3);
			searchService.Add(page4);

			// Act
			List<SearchResultViewModel> results = searchService.Search("id:1").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(1));
		}

		[Test]
		public void CreatedBy_Only_Searchable_Using_Field_Syntax()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "random name2", "homepage, tag1", "title content 11");
			PageViewModel page2 = CreatePage(2, "admin", "random name2", "tag1, tag", "title content 2");

			searchService.Add(page1);
			searchService.Add(page2);

			// Act
			List<SearchResultViewModel> results = searchService.Search("admin").ToList();
			List<SearchResultViewModel> createdByResults = searchService.Search("createdby: admin").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(0), "admin title count");
			Assert.That(createdByResults.Count, Is.EqualTo(2), "createdby count");
		}

		[Test]
		public void Search_By_CreatedOnField_Returns_Results()
		{
			// Arrange
			string todaysDate = DateTime.Today.ToShortDateString(); // (SearchService stores dates, not times)
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "random name2", "homepage, tag1", "title content", DateTime.Today);
			PageViewModel page2 = CreatePage(2, "admin", "random name2", "tag1, tag", "title content", DateTime.Today);
			PageViewModel page3 = CreatePage(3, "admin", "random name3", "tag1, tag", "title content", DateTime.Today.AddDays(1));
			PageViewModel page4 = CreatePage(4, "admin", "random name4", "tag1, tag", "title content", DateTime.Today.AddDays(2));

			searchService.Add(page1);
			searchService.Add(page2);
			searchService.Add(page3);
			searchService.Add(page4);

			// Act
			List<SearchResultViewModel> createdOnResults = searchService.Search("createdon:" +todaysDate).ToList();

			// Assert
			Assert.That(createdOnResults.Count, Is.EqualTo(2), "createdon count");
		}

		[Test]
		public void Delete_Should_Remove_Page_From_Index()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "homepage title", "homepage1, tag1", "title content");
			PageViewModel page2 = CreatePage(2, "admin", "random name2", "tag1", "random name 2");

			searchService.Add(page1);
			searchService.Add(page2);

			// Act
			searchService.Delete(page1);
			List<SearchResultViewModel> results = searchService.Search("homepage title").ToList();

			// Assert
			Assert.That(results.Count, Is.EqualTo(0), "homepage title still appears after deletion");
		}

		[Test]
		public void Update_Should_Show_In_Index_Search()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "homepage title", "homepage1, tag1", "title content");
			PageViewModel page2 = CreatePage(2, "admin", "random name2", "tag1", "random name 2");

			searchService.Add(page1);
			searchService.Add(page2);

			// Act
			page1.Title = "A new hope";
			searchService.Update(page1);

			Thread thread = new Thread(delegate()
			{
				// Perform the test in a new thread, so that the add + delete commit is picked up
				// which is periodically done by Lucene.
				List<SearchResultViewModel> oldResults = searchService.Search("homepage title").ToList();
				List<SearchResultViewModel> newResults = searchService.Search("A new hope").ToList();

				// Assert
				Assert.That(oldResults.Count, Is.EqualTo(0), "old results");
				Assert.That(newResults.Count, Is.EqualTo(1), "new results");
			});
			thread.Start();
		}

		[Test]
		[Description("See bug #93")]
		public void Cyrillic_Content_Should_Be_Stored_And_Retrieved_Correctly()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "ОШИБКА: неверная последовательность байт для кодировки", "tag1", 
				"БД сервера событий была перенесена из PostgreSQL 8.3 на PostgreSQL 9.1.4. Сервер, развернутый на Windows платформе, не мог с ней работать, т.к. установщик PostgreSQL 9.1.4 создает шаблон базы с использованием кодировки UTF8 и, сответственно, новая БД не могла быть создана с требуемой");

			searchService.Add(page1);

			// Act
			List<SearchResultViewModel> results = searchService.Search("ОШИБКА").ToList();

			// Assert
			Assert.That(results[0].ContentSummary, Contains.Substring("БД сервера событий была перенесена из"));
		}

		[Test]
		[Description("Not an integration test, but grouped here for convenience.")]
		public void GetContentSummary_Should_Only_Contain_First_150_Characters_For_Summary()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "A page title", "tag1", "Lorizzle ipsizzle dolor sit amizzle, (pre character 150 boundary) rizzle adipiscing tellivizzle. Nullizzle sapizzle velizzle, yo mamma volutpat, suscipizzle bow wow wow, gravida vizzle, (post 150 character boundary) shizznit. Pellentesque da bomb tortizzle. Hizzle erizzle. Its fo rizzle izzle sheezy dapibizzle mofo tempizzle tempizzle. Maurizzle away nibh izzle turpis. Phat izzle hizzle. Pellentesque eleifend rhoncus rizzle. Da bomb things dang platea dictumst. Fo shizzle my nizzle dapibizzle. Shiz tellus owned, pretizzle eu, mattizzle ac, bow wow wow its fo rizzle, nunc. Shiz suscipit. Integizzle own yo' we gonna chung sed go to hizzle.");
			searchService.Add(page1);

			// Act
			List<SearchResultViewModel> results = searchService.Search("rizzle").ToList();

			// Assert
			Assert.That(results[0].ContentSummary, Contains.Substring("(pre character 150 boundary)"));
			Assert.That(results[0].ContentSummary, Is.Not.StringContaining("(post character 150 boundary)"));

		}

		[Test]
		[Description("Not an integration test, but grouped here for convenience.")]
		public void GetContentSummary_Should_Remove_Html_From_Summary()
		{
			// Arrange
			SearchService searchService = new SearchService(_config, _repository, _pluginFactory);
			searchService.CreateIndex();

			PageViewModel page1 = CreatePage(1, "admin", "A page title", "tag1", "**some bold** \n\n=my header=");
			searchService.Add(page1);

			// Act
			List<SearchResultViewModel> results = searchService.Search("my header").ToList();

			// Assert
			Assert.That(results[0].ContentSummary, Is.Not.StringContaining("<b>some bold</b>"));
			Assert.That(results[0].ContentSummary, Is.Not.StringContaining("<h1>my header</h1>"));

		}

		protected PageViewModel CreatePage(int id, string createdBy, string title, string tags, string textContent = "", DateTime? createdOn = null)
		{
			if (createdOn == null)
				createdOn = DateTime.UtcNow;

			return new PageViewModel()
			{
				Id = id,
				CreatedBy = createdBy,
				Title = title,
				RawTags = tags,
				Content = textContent,
				CreatedOn = createdOn.Value,
				ModifiedBy = createdBy,
				ModifiedOn = createdOn.Value,
				VersionNumber = 1
			};
		}
	}
}
