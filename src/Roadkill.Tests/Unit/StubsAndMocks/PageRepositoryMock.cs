using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Database;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class PageRepositoryMock : IPageRepository
	{
		public List<Page> Pages { get; set; }
		public List<PageContent> PageContents { get; set; }

		public PageRepositoryMock()
		{
			Pages = new List<Page>();
			PageContents = new List<PageContent>();
		}

		public void DeletePage(Page page)
		{
			Pages.Remove(page);
		}

		public void DeletePageContent(PageContent pageContent)
		{
			PageContents.Remove(pageContent);
		}

		public void DeleteAllPages()
		{
			Pages = new List<Page>();
			PageContents = new List<PageContent>();
		}

		public Page SaveOrUpdatePage(Page page)
		{
			Page existingPage = Pages.FirstOrDefault(x => x.Id == page.Id);

			if (existingPage == null)
			{
				page.Id = Pages.Count + 1;
				Pages.Add(page);
				existingPage = page;
			}
			else
			{
				existingPage.CreatedBy = page.CreatedBy;
				existingPage.CreatedOn = page.CreatedOn;
				existingPage.IsLocked = page.IsLocked;
				existingPage.ModifiedBy = page.ModifiedBy;
				existingPage.ModifiedOn = page.ModifiedOn;
				existingPage.Tags = page.Tags;
				existingPage.Title = page.Title;
			}

			return existingPage;
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			page.Id = Pages.Count + 1;
			Pages.Add(page);

			PageContent content = new PageContent();
			content.Id = Guid.NewGuid();
			content.EditedBy = editedBy;
			content.EditedOn = editedOn;
			content.Page = page;
			content.Text = text;
			content.VersionNumber = 1;
			PageContents.Add(content);

			return content;
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			PageContent content = new PageContent();
			content.Id = Guid.NewGuid();
			page.ModifiedBy = content.EditedBy = editedBy;
			page.ModifiedOn = content.EditedOn = editedOn;
			content.Page = page;
			content.Text = text;
			content.VersionNumber = FindPageContentsByPageId(page.Id).Max(x => x.VersionNumber) +1;
			PageContents.Add(content);

			return content;
		}

		public void UpdatePageContent(PageContent content)
		{
			PageContent existingContent = PageContents.FirstOrDefault(x => x.Id == content.Id);

			if (existingContent == null)
			{
				// Do nothing
			}
			else
			{
				existingContent.EditedOn = content.EditedOn;
				existingContent.EditedBy = content.EditedBy;
				existingContent.Text = content.Text;
				existingContent.VersionNumber = content.VersionNumber;
			}
		}

		public IEnumerable<Page> AllPages()
		{
			return Pages;
		}

		public Page GetPageById(int id)
		{
			return Pages.FirstOrDefault(p => p.Id == id);
		}

		public IEnumerable<Page> FindPagesCreatedBy(string username)
		{
			return Pages.Where(p => p.CreatedBy == username);
		}

		public IEnumerable<Page> FindPagesModifiedBy(string username)
		{
			return Pages.Where(p => p.ModifiedBy == username);
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			return Pages.Where(p => p.Tags.ToLower().Contains(tag.ToLower()));
		}

		public IEnumerable<string> AllTags()
		{
			return Pages.Select(x => x.Tags);
		}

		public Page GetPageByTitle(string title)
		{
			return Pages.FirstOrDefault(p => p.Title == title);
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			return PageContents.Where(p => p.Page.Id == pageId).OrderByDescending(x => x.EditedOn).FirstOrDefault();
		}

		public PageContent GetPageContentById(Guid id)
		{
			return PageContents.FirstOrDefault(p => p.Id == id);
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			return PageContents.FirstOrDefault(p => p.Page.Id == id && p.VersionNumber == versionNumber);
		}

		public PageContent GetPageContentByVersionId(Guid versionId)
		{
			return PageContents.FirstOrDefault(p => p.Id == versionId);
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			return PageContents.Where(p => p.Page.Id == pageId).ToList();
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			return PageContents.Where(p => p.EditedBy == username);
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			return PageContents;
		}

		public void Dispose()
		{
			
		}
	}
}
