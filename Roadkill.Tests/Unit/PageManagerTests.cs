using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;
using NHibernate;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Controllers;
using Roadkill.Core.Search;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PageManagerTests : MoqTestBase
	{
		[SetUp]
		public void SearchSetup()
		{
			RoadkillApplication.SetupIoC();
		}

		[Test]
		public void AddPage_Should_Save_To_Repository()
		{
			// Arrange
			// - Track the repository save events, including IQuery
			Mock<Page> page = AddAndGetMockPage(1, "admin", "Homepage", "homepage",  DateTime.Today);
			PageSummary summary = page.Object.ToSummary();
			page.Setup(x => x.ToSummary()).Returns(summary);
			_mockRepository.Setup(x => x.SaveOrUpdate<Page>(page.Object));
			_mockRepository.Setup(x => x.SaveOrUpdate<PageSummary>(summary));
			_queryMock.Setup(x => x.UniqueResult<PageContent>()).Returns(page.Object.CurrentContent());

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			PageSummary newSummary = manager.AddPage(summary);

			// Assert
			Assert.That(newSummary, Is.Not.Null);
			Assert.That(newSummary.Content, Is.EqualTo(summary.Content));
			_mockRepository.Verify(x => x.SaveOrUpdate<Page>(It.IsAny<Page>()));
			_mockRepository.Verify(x => x.SaveOrUpdate<PageContent>(It.IsAny<PageContent>()));
		}

		[Test]
		public void AllTags_Should_Return_Correct_Items()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");
			Page page3 = AddMockPage(3, "admin", "page 3", "page3");
			Page page4 = AddMockPage(4, "admin", "page 4", "animals");
			Page page5 = AddMockPage(5, "admin", "page 5", "animals");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			List<TagSummary> summaries = manager.AllTags().OrderBy(t => t.Name).ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(4), "Tag summary count");
			Assert.That(summaries[0].Name, Is.EqualTo("animals"));
			Assert.That(summaries[1].Name, Is.EqualTo("homepage"));
			Assert.That(summaries[2].Name, Is.EqualTo("page2"));
			Assert.That(summaries[3].Name, Is.EqualTo("page3"));
		}

		[Test]
		public void DeletePage_Should_Remove_Correct_Page()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");
			Page page3 = AddMockPage(3, "admin", "page 3", "page3");
			Page page4 = AddMockPage(4, "admin", "page 4", "animals");
			Page page5 = AddMockPage(5, "admin", "page 5", "animals");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			manager.DeletePage(page1.Id);
			manager.DeletePage(page2.Id);
			List<PageSummary> summaries = manager.AllPages().ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(3), "Page count");
			Assert.That(summaries.FirstOrDefault(p => p.Title == "Homepage"), Is.Null);
			Assert.That(summaries.FirstOrDefault(p => p.Title == "page 2"), Is.Null);
		}

		[Test]
		public void AllPages_CreatedBy_Should_Have_Correct_Authors()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");
			Page page3 = AddMockPage(3, "bob", "page 3", "page3");
			Page page4 = AddMockPage(4, "bob", "page 4", "animals");
			Page page5 = AddMockPage(5, "bob", "page 5", "animals");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			List<PageSummary> summaries = manager.AllPagesCreatedBy("bob").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(3), "Summary count");
			Assert.That(summaries.FirstOrDefault(p => p.CreatedBy == "admin"), Is.Null);
		}

		[Test]
		public void AllPages_Should_Have_Correct_Items()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");
			Page page3 = AddMockPage(3, "bob", "page 3", "page3");
			Page page4 = AddMockPage(4, "bob", "page 4", "animals");
			Page page5 = AddMockPage(5, "bob", "page 5", "animals");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			List<PageSummary> summaries = manager.AllPages().ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(5), "Summary count");
		}

		[Test]
		public void FindByTags_For_Single_Tag_Returns_Single_Result()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");
			Page page3 = AddMockPage(3, "admin", "page 3", "page3");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			List<PageSummary> summaries = manager.FindByTag("homepage").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(1), "Summary count");
			Assert.That(summaries[0].Title, Is.EqualTo("Homepage"), "Summary title");
			Assert.That(summaries[0].Tags, Is.EqualTo("homepage"), "Summary tags");
		}

		[Test]
		public void FindByTags_For_Multiple_Tags_Returns_Many_Results()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");
			Page page3 = AddMockPage(3, "admin", "page 3", "page3");
			Page page4 = AddMockPage(4, "admin", "page 4", "animals");
			Page page5 = AddMockPage(5, "admin", "page 5", "animals");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			List<PageSummary> summaries = manager.FindByTag("animals").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(2), "Summary count");
		}

		[Test]
		public void FindByTitle_Should_Return_Correct_Page()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");
			Page page3 = AddMockPage(3, "bob", "page 3", "page3");
			Page page4 = AddMockPage(4, "bob", "page 4", "animals");
			Page page5 = AddMockPage(5, "bob", "page 5", "animals");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			PageSummary summary = manager.FindByTitle("page 3");

			// Assert
			Assert.That(summary.Title, Is.EqualTo("page 3"), "Page title");
		}

		[Test]
		public void Get_Should_Return_Correct_Page()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");
			Page page3 = AddMockPage(3, "bob", "page 3", "page3");
			Page page4 = AddMockPage(4, "bob", "page 4", "animals");
			Page page5 = AddMockPage(5, "bob", "page 5", "animals");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			PageSummary summary = manager.GetById(page3.Id);

			// Assert
			Assert.That(summary.Id, Is.EqualTo(page3.Id), "Page id");
			Assert.That(summary.Title, Is.EqualTo("page 3"), "Page title");
		}

		[Test]
		public void ExportToXml_Should_Contain_Xml()
		{
			// Arrange
			Page page1 = AddMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = AddMockPage(2, "admin", "page 2", "page2");

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			string xml = manager.ExportToXml();

			// Assert
			Assert.That(xml, Is.StringContaining("<?xml"));
			Assert.That(xml, Is.StringContaining("<ArrayOfPageSummary"));
			Assert.That(xml, Is.StringContaining("<Id>1</Id>"));
			Assert.That(xml, Is.StringContaining("<Id>2</Id>"));
		}

		[Test]
		public void RenameTags_For_Multiple_Tags_Returns_Many_Results()
		{
			// Arrange
			// Ensure saves (called for tag renamed) are called with our page objects
			Mock<Page> page = AddAndGetMockPage(1, "admin", "Homepage", "animal; tag2;", DateTime.Today);
			PageSummary summary = page.Object.ToSummary();
			page.Setup(x => x.ToSummary()).Returns(summary);
			_mockRepository.Setup(x => x.SaveOrUpdate<Page>(page.Object));
			_mockRepository.Setup(x => x.SaveOrUpdate<PageSummary>(summary));
			_mockSearchManager.Setup(x => x.Delete(It.IsAny<PageSummary>())); // used for the search index updates
			
			Mock<Page> page2 = AddAndGetMockPage(2, "admin", "Page 2", "animal; tag2;", DateTime.Today);
			PageSummary summary2 = page2.Object.ToSummary();
			page2.Setup(x => x.ToSummary()).Returns(summary2);
			_mockRepository.Setup(x => x.SaveOrUpdate<Page>(page2.Object));
			_mockRepository.Setup(x => x.SaveOrUpdate<PageSummary>(summary2));

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);			
			manager.RenameTag("animal", "vegetable");
			List<PageSummary> animalTagList = manager.FindByTag("animal").ToList();
			List<PageSummary> vegetableTagList = manager.FindByTag("vegetable").ToList();

			// Assert
			Assert.That(animalTagList.Count, Is.EqualTo(0), "Old tag summary count");
			Assert.That(vegetableTagList.Count, Is.EqualTo(2), "New tag summary count");
		}

		[Test]
		public void UpdatePage_Should_Persist_To_Repository()
		{
			// Arrange
			// - Ensure saves are called using our page object
			Mock<Page> page = AddAndGetMockPage(1, "admin", "Homepage", "tag1; tag2", DateTime.Today);
			PageSummary summary = page.Object.ToSummary();
			page.Setup(x => x.ToSummary()).Returns(summary);
			_mockRepository.Setup(x => x.SaveOrUpdate<Page>(page.Object));
			_mockRepository.Setup(x => x.SaveOrUpdate<PageSummary>(summary));

			// Act
			PageManager manager = new PageManager(_mockSearchManager.Object);
			summary.Title = "New title";
			summary.Tags = "New; tags";
			summary.Content = "New content";
			summary.CreatedBy = "Newuser";
			summary.CreatedOn = DateTime.Today.AddDays(-1);
			summary.ModifiedBy = "Newuser";
			summary.ModifiedOn = DateTime.Today.AddDays(-1);
			summary.Tags = "tag1; tag2;"; // TODO: sort this out so it's a method
			manager.UpdatePage(summary);
			PageSummary actual = manager.GetById(1);

			// Assert
			Assert.That(actual.Title, Is.EqualTo(summary.Title), "Title");
			Assert.That(actual.Tags, Is.EqualTo(summary.Tags), "Tags");
			_mockRepository.Verify(x => x.SaveOrUpdate<Page>(It.IsAny<Page>()));
			_mockRepository.Verify(x => x.SaveOrUpdate<PageContent>(It.IsAny<PageContent>()));
		}
	}
}
