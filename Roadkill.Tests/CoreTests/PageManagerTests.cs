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

namespace Roadkill.Tests.Controllers
{
	[TestFixture]
	public class PageManagerTests : MoqTestBase
	{
		[Test]
		public void AddPage_Should_Call_NHibernate()
		{
			// Arrange
			Mock<Page> page = CreateAndGetMockPage(1, "admin", "Homepage", "homepage",  DateTime.Today);
			PageSummary summary = page.Object.ToSummary(page.Object.CurrentContent());
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
		public void FindByTags_For_Single_Tag_Returns_Single_Result()
		{
			// Arrange
			Page page1 = CreateMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = CreateMockPage(2, "admin", "page 2", "page2");
			Page page3 = CreateMockPage(3, "admin", "page 3", "page3");

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
			Page page1 = CreateMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = CreateMockPage(2, "admin", "page 2", "page2");
			Page page3 = CreateMockPage(3, "admin", "page 3", "page3");
			Page page4 = CreateMockPage(4, "admin", "page 4", "animals");
			Page page5 = CreateMockPage(5, "admin", "page 5", "animals");

			// Act
			PageManager manager = new PageManager();
			List<PageSummary> summaries = manager.FindByTag("animals").ToList();

			// Assert
			Assert.That(summaries.Count, Is.EqualTo(2), "Summary count");
		}

		[Test]
		public void AllTags_Should_Return_Correct_Items()
		{
			// Arrange
			Page page1 = CreateMockPage(1, "admin", "Homepage", "homepage");
			Page page2 = CreateMockPage(2, "admin", "page 2", "page2");
			Page page3 = CreateMockPage(3, "admin", "page 3", "page3");
			Page page4 = CreateMockPage(4, "admin", "page 4", "animals");
			Page page5 = CreateMockPage(5, "admin", "page 5", "animals");

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
	}
}
