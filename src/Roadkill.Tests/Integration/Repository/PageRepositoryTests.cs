using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Unit")]
	public abstract class PageRepositoryTests : RepositoryTests
	{
		private Page _page1;
		private PageContent _pageContent1;
		private PageContent _pageContent2;
		private DateTime _createdDate;
		private DateTime _editedDate;

		[SetUp]
		public void SetUp()
		{
			// Create 5 Pages with 2 versions of content each
			_createdDate = DateTime.Today.ToUniversalTime().AddDays(-1);
			_editedDate = _createdDate.AddHours(1);

			_page1 = NewPage("admin", "homepage, newpage");
			_pageContent1 = Repository.AddNewPage(_page1, "text", "admin", _createdDate);
			_page1 = _pageContent1.Page;
			_pageContent2 = Repository.AddNewPageContentVersion(_page1, "v2", "admin", _editedDate, 2);
			_page1 = _pageContent2.Page; // update the modified date

			Page page2 = NewPage("editor1");
			PageContent pageContent2 = Repository.AddNewPage(page2, "text", "editor1", _createdDate);
			Repository.AddNewPageContentVersion(pageContent2.Page, "v2", "editor1", _editedDate, 1);

			Page page3 = NewPage("editor2");
			PageContent pageContent3 = Repository.AddNewPage(page3, "text", "editor2", _createdDate);
			Repository.AddNewPageContentVersion(pageContent3.Page, "v2", "editor2", _editedDate, 1);

			Page page4 = NewPage("editor3");
			PageContent pageContent4 = Repository.AddNewPage(page4, "text", "editor3", _createdDate);
			Repository.AddNewPageContentVersion(pageContent4.Page, "v2", "editor3", _editedDate, 1);

			Page page5 = NewPage("editor4");
			PageContent pageContent5 = Repository.AddNewPage(page5, "text", "editor4", _createdDate);
			Repository.AddNewPageContentVersion(pageContent5.Page, "v2", "editor4", _editedDate, 1);
		}

		protected Page NewPage(string author, string tags = "tag1,tag2,tag3", string title = "Title")
		{
			Page page = new Page()
			{
				Title = title,
				CreatedOn = _createdDate,
				CreatedBy = author,
				ModifiedBy = author,
				ModifiedOn = _createdDate,
				Tags = tags
			};

			return page;
		}

		[Test]
		public void AllPages()
		{
			// Arrange


			// Act
			List<Page> actualList = Repository.AllPages().ToList();

			// Assert
			Assert.That(actualList.Count, Is.EqualTo(5));
			Assert.That(actualList[0], Is.Not.Null);
			Assert.That(actualList[1], Is.Not.Null);
			Assert.That(actualList[2], Is.Not.Null);
			Assert.That(actualList[3], Is.Not.Null);
			Assert.That(actualList[4], Is.Not.Null);
		}

		[Test]
		public void GetPageById()
		{
			// Arrange


			// Act
			Page actualPage = Repository.GetPageById(_page1.Id);

			// Assert
			Assert.That(actualPage, Is.Not.Null);
			Assert.That(actualPage.Id, Is.EqualTo(_page1.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(_page1.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(_page1.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(_page1.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(_page1.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(_page1.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(_page1.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(_page1.Title));
		}

		[Test]
		public void FindPagesCreatedBy()
		{
			// Arrange


			// Act
			List<Page> actualPages = Repository.FindPagesCreatedBy("admin").ToList();

			// Assert
			Assert.That(actualPages.Count, Is.EqualTo(1));
			Assert.That(actualPages[0].Id, Is.EqualTo(_page1.Id));
			Assert.That(actualPages[0].CreatedBy, Is.EqualTo(_page1.CreatedBy));
			Assert.That(actualPages[0].CreatedOn, Is.EqualTo(_page1.CreatedOn));
			Assert.That(actualPages[0].IsLocked, Is.EqualTo(_page1.IsLocked));
			Assert.That(actualPages[0].ModifiedBy, Is.EqualTo(_page1.ModifiedBy));
			Assert.That(actualPages[0].ModifiedOn, Is.EqualTo(_page1.ModifiedOn));
			Assert.That(actualPages[0].Tags, Is.EqualTo(_page1.Tags));
			Assert.That(actualPages[0].Title, Is.EqualTo(_page1.Title));
		}

		[Test]
		public void FindPagesByModifiedBy()
		{
			// Arrange
			PageContent newContent = Repository.AddNewPageContentVersion(_page1, "new text", "bob", _createdDate, 3);
			Page expectedPage = newContent.Page;

			// Act
			List<Page> actualPages = Repository.FindPagesModifiedBy("bob").ToList();

			// Assert
			Assert.That(actualPages.Count, Is.EqualTo(1));
			Assert.That(actualPages[0].Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPages[0].CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPages[0].CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPages[0].IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPages[0].ModifiedBy, Is.EqualTo("bob"));
			Assert.That(actualPages[0].ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPages[0].Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPages[0].Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void FindPagesContainingTag()
		{
			// Arrange


			// Act
			List<Page> actualPages = Repository.FindPagesContainingTag("tag1").ToList();


			// Assert
			Assert.That(actualPages.Count, Is.EqualTo(4));
		}

		[Test]
		public void AllTags()
		{
			// Arrange


			// Act
			List<string> actual = Repository.AllTags().ToList();

			// Assert
			Assert.That(actual.Count, Is.EqualTo(5)); // homepage, newpage, tag1, tag2, tag3
		}

		[Test]
		public void GetPageByTitle()
		{
			// Arrange
			string title = "page title";
			Page expectedPage = NewPage("admin", "tag1", title);
			PageContent newContent = Repository.AddNewPage(expectedPage, "sometext", "admin", _createdDate);
			expectedPage.Id = newContent.Page.Id; // get the new identity

			// Act
			Page actualPage = Repository.GetPageByTitle(title);

			// Assert
			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(expectedPage.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void GetLatestPageContent()
		{
			// Arrange
			PageContent expectedContent = _pageContent2;
			Page expectedPage = _pageContent2.Page;

			// Act
			PageContent actualContent = Repository.GetLatestPageContent(_pageContent2.Page.Id);
			Page actualPage = actualContent.Page;

			// Assert
			Assert.That(actualContent.EditedBy, Is.EqualTo(expectedContent.EditedBy));
			Assert.That(actualContent.EditedOn, Is.EqualTo(expectedContent.EditedOn));
			Assert.That(actualContent.Id, Is.EqualTo(expectedContent.Id));
			Assert.That(actualContent.Text, Is.EqualTo(expectedContent.Text));
			Assert.That(actualContent.VersionNumber, Is.EqualTo(expectedContent.VersionNumber));

			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(expectedPage.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void GetPageContentById()
		{
			// Arrange
			PageContent expectedContent = _pageContent2;
			Page expectedPage = _pageContent2.Page;

			// Act
			PageContent actualContent = Repository.GetPageContentById(expectedContent.Id);
			Page actualPage = actualContent.Page;

			// Assert
			Assert.That(actualContent.EditedBy, Is.EqualTo(expectedContent.EditedBy));
			Assert.That(actualContent.EditedOn, Is.EqualTo(expectedContent.EditedOn));
			Assert.That(actualContent.Id, Is.EqualTo(expectedContent.Id));
			Assert.That(actualContent.Text, Is.EqualTo(expectedContent.Text));
			Assert.That(actualContent.VersionNumber, Is.EqualTo(expectedContent.VersionNumber));

			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(expectedPage.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void GetPageContentByPageIdAndVersionNumber()
		{
			// Arrange
			PageContent expectedContent = _pageContent2;
			Page expectedPage = _pageContent2.Page;

			// Act
			PageContent actualContent = Repository.GetPageContentByPageIdAndVersionNumber(expectedPage.Id, expectedContent.VersionNumber);
			Page actualPage = actualContent.Page;

			// Assert
			Assert.That(actualContent.EditedBy, Is.EqualTo(expectedContent.EditedBy));
			Assert.That(actualContent.EditedOn, Is.EqualTo(expectedContent.EditedOn));
			Assert.That(actualContent.Id, Is.EqualTo(expectedContent.Id));
			Assert.That(actualContent.Text, Is.EqualTo(expectedContent.Text));
			Assert.That(actualContent.VersionNumber, Is.EqualTo(expectedContent.VersionNumber));

			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
			Assert.That(actualPage.CreatedBy, Is.EqualTo(expectedPage.CreatedBy));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(expectedPage.CreatedOn));
			Assert.That(actualPage.IsLocked, Is.EqualTo(expectedPage.IsLocked));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo(expectedPage.ModifiedBy));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(expectedPage.ModifiedOn));
			Assert.That(actualPage.Tags, Is.EqualTo(expectedPage.Tags));
			Assert.That(actualPage.Title, Is.EqualTo(expectedPage.Title));
		}

		[Test]
		public void GetPageContentByEditedBy()
		{
			// Arrange

			// Act
			List<PageContent> allContent = Repository.GetPageContentByEditedBy("admin").ToList();

			// Assert
			Assert.That(allContent.Count, Is.EqualTo(2));
		}

		[Test]
		public void FindPageContentsByPageId()
		{
			// Arrange


			// Act
			List<PageContent> pagesContents = Repository.FindPageContentsByPageId(_page1.Id).ToList();

			// Assert
			Assert.That(pagesContents.Count, Is.EqualTo(2));
			Assert.That(pagesContents[0], Is.Not.Null);

			PageContent expectedPageContent = pagesContents.FirstOrDefault(x => x.Id == _pageContent1.Id);
			Assert.That(expectedPageContent, Is.Not.Null);
		}

		[Test]
		public void FindPageContentsEditedBy()
		{
			// Arrange


			// Act
			List<PageContent> pagesContents = Repository.FindPageContentsEditedBy("admin").ToList();

			// Assert
			Assert.That(pagesContents.Count, Is.EqualTo(2));
			Assert.That(pagesContents[0], Is.Not.Null);

			PageContent expectedPageContent = pagesContents.FirstOrDefault(x => x.Id == _pageContent1.Id);
			Assert.That(expectedPageContent, Is.Not.Null);
		}

		[Test]
		public void AllPageContents()
		{
			// Arrange


			// Act
			List<PageContent> pagesContents = Repository.AllPageContents().ToList();

			// Assert
			Assert.That(pagesContents.Count, Is.EqualTo(10)); // five pages with 2 versions
			Assert.That(pagesContents[0], Is.Not.Null);

			PageContent expectedPageContent = pagesContents.FirstOrDefault(x => x.Id == _pageContent1.Id);
			Assert.That(expectedPageContent, Is.Not.Null);
		}

		[Test]
		public void DeletePage_Test()
		{
			// Arrange
			Page page = Repository.GetPageById(1);

			// Act
			Repository.DeletePage(page);

			// Assert
			Assert.That(page, Is.Not.Null);
			Assert.That(Repository.GetPageById(1), Is.Null);
		}

		[Test]
		public void DeletePageContent()
		{
			// Arrange
			PageContent pageContent = Repository.GetLatestPageContent(1);
			Guid id = pageContent.Id;

			// Act
			Repository.DeletePageContent(pageContent);

			// Assert
			Assert.That(Repository.GetPageContentById(id), Is.Null);
		}


		[Test]
		public void SaveOrUpdatePage()
		{
			// Arrange
			Page newPage = NewPage("admin", "tag1, 3, 4");
			DateTime modifiedDate = _createdDate.AddMinutes(1);

			Page existingPage = _page1;
			existingPage.Title = "new title";
			existingPage.ModifiedBy = "editor1";
			existingPage.ModifiedOn = modifiedDate;

			// Act
			Repository.SaveOrUpdatePage(newPage);
			Repository.SaveOrUpdatePage(existingPage);

			// Assert
			Assert.That(Repository.AllPages().Count(), Is.EqualTo(6));

			Page actualPage = Repository.GetPageById(existingPage.Id);
			Assert.That(actualPage.Title, Is.EqualTo("new title"));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo("editor1"));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(modifiedDate));
		}

		[Test]
		public void AddNewPage()
		{
			// Arrange
			Page newPage = NewPage("admin", "tag1,3,4");
			newPage.ModifiedOn = _createdDate;

			// Act
			PageContent pageContent = Repository.AddNewPage(newPage, "my text", "admin", _createdDate);

			// Assert
			Assert.That(Repository.AllPages().Count(), Is.EqualTo(6));
			Assert.That(pageContent, Is.Not.Null);
			Assert.That(pageContent.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(pageContent.Text, Is.EqualTo("my text"));
			Assert.That(pageContent.EditedOn, Is.EqualTo(_createdDate));
			Assert.That(pageContent.VersionNumber, Is.EqualTo(1));

			Page actualPage = Repository.GetPageById(pageContent.Page.Id);
			Assert.That(actualPage.Title, Is.EqualTo("Title"));
			Assert.That(actualPage.Tags, Is.EqualTo("tag1,3,4"));
			Assert.That(actualPage.CreatedOn, Is.EqualTo(_createdDate));
			Assert.That(actualPage.CreatedBy, Is.EqualTo("admin"));
			Assert.That(actualPage.ModifiedBy, Is.EqualTo("admin"));
			Assert.That(actualPage.ModifiedOn, Is.EqualTo(_createdDate));
		}

		[Test]
		public void AddNewPageContentVersion()
		{
			// Arrange
			Page existingPage = _page1;

			// Act
			PageContent newContent = Repository.AddNewPageContentVersion(existingPage, "new text", "admin", _createdDate, 2);

			// Assert
			Assert.That(Repository.AllPageContents().Count(), Is.EqualTo(11));
			Assert.That(newContent, Is.Not.Null);
			Assert.That(newContent.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(newContent.Text, Is.EqualTo("new text"));
			Assert.That(newContent.EditedOn, Is.EqualTo(_createdDate));
			Assert.That(newContent.VersionNumber, Is.EqualTo(2));

			PageContent latestContent = Repository.GetPageContentById(newContent.Id);
			Assert.That(latestContent.Id, Is.EqualTo(newContent.Id));
			Assert.That(latestContent.Text, Is.EqualTo(newContent.Text));
			Assert.That(latestContent.EditedOn, Is.EqualTo(newContent.EditedOn));
			Assert.That(latestContent.VersionNumber, Is.EqualTo(newContent.VersionNumber));
		}

		[Test]
		public void UpdatePageContent()
		{
			// Arrange
			DateTime editedDate = _editedDate.AddMinutes(10);

			PageContent existingContent = _pageContent1;
			int versionNumber = 2;
			int pageId = existingContent.Page.Id;

			existingContent.Text = "new text";
			existingContent.EditedBy = "editor1";
			existingContent.EditedOn = editedDate;
			existingContent.VersionNumber = versionNumber;

			// Act
			Repository.UpdatePageContent(existingContent);
			PageContent actualContent = Repository.GetPageContentById(existingContent.Id);

			// Assert
			Assert.That(actualContent, Is.Not.Null);
			Assert.That(actualContent.Text, Is.EqualTo("new text"));
			Assert.That(actualContent.EditedBy, Is.EqualTo("editor1"));
			Assert.That(actualContent.EditedOn, Is.EqualTo(editedDate));
			Assert.That(actualContent.VersionNumber, Is.EqualTo(versionNumber));
		}

		public void DeleteAllPages()
		{
			// Arrange


			// Act
			Repository.DeleteAllPages();

			// Assert
			Assert.That(Repository.AllPages().Count(), Is.EqualTo(0));
			Assert.That(Repository.AllPageContents().Count(), Is.EqualTo(0));
		}
	}
}
