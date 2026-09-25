using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Core.Database.MongoDB
{
	public class MongoDBPageRepository : IPageRepository
	{
		internal readonly string ConnectionString;

		public IQueryable<Page> Pages => Queryable<Page>();
		public IQueryable<PageContent> PageContents => Queryable<PageContent>();

		public MongoDBPageRepository(string connectionString)
		{
			ConnectionString = connectionString;
		}

		public void Wipe()
		{
			MongoDbStore.Wipe(ConnectionString);
		}

		public void Delete<T>(T obj) where T : IDataStoreEntity
		{
			MongoDbStore.Delete(ConnectionString, obj);
		}

		public void DeleteAll<T>() where T : IDataStoreEntity
		{
			MongoDbStore.DeleteAll<T>(ConnectionString);
		}

		public IQueryable<T> Queryable<T>() where T : IDataStoreEntity
		{
			return MongoDbStore.Queryable<T>(ConnectionString);
		}

		public void SaveOrUpdate<T>(T obj) where T : IDataStoreEntity
		{
			// Implement autoincrement identity(1,1) for MongoDB, for Page objects
			Page page = obj as Page;
			if (page != null && page.Id == 0)
			{
				int newId = 1;
				Page recentPage = Queryable<Page>().OrderByDescending(x => x.Id).FirstOrDefault();
				if (recentPage != null)
				{
					newId = recentPage.Id + 1;
				}

				obj.ObjectId = Guid.NewGuid();
				page.Id = newId;
			}

			MongoDbStore.SaveOrUpdate(ConnectionString, obj);
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			return Queryable<PageContent>()
				.Where(x => x.Page.Id == pageId)
				.OrderByDescending(x => x.EditedOn)
				.FirstOrDefault();
		}

		public IEnumerable<Page> AllPages()
		{
			return Pages.ToList();
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
			return new List<string>(Pages.Select(p => p.Tags));
		}

		public Page GetPageByTitle(string title)
		{
			if (string.IsNullOrEmpty(title))
				return null;

			// Case insensitive, as with SQL Server and Postgres
			string lowerTitle = title.ToLowerInvariant();
			return Pages.FirstOrDefault(p => p.Title.ToLower() == lowerTitle);
		}

		public PageContent GetPageContentById(Guid id)
		{
			return PageContents.FirstOrDefault(p => p.Id == id);
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			return PageContents.FirstOrDefault(p => p.Page.Id == id && p.VersionNumber == versionNumber);
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			return PageContents.Where(p => p.Page.Id == pageId);
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			return PageContents.ToList();
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			return PageContents.Where(p => p.EditedBy == username);
		}

		public void DeletePage(Page page)
		{
			Delete<Page>(page);
		}

		public void DeletePageContent(PageContent pageContent)
		{
			Delete<PageContent>(pageContent);
		}

		public void DeleteAllPages()
		{
			DeleteAll<Page>();
			DeleteAll<PageContent>();
		}

		public Page SaveOrUpdatePage(Page page)
		{
			SaveOrUpdate<Page>(page);
			return page;
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			SaveOrUpdate<Page>(page);

			PageContent pageContent = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = 1,
			};

			SaveOrUpdate<PageContent>(pageContent);
			return pageContent;
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			page.ModifiedOn = editedOn;
			page.ModifiedBy = editedBy;
			SaveOrUpdate<Page>(page);

			PageContent pageContent = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = version,
			};

			SaveOrUpdate<PageContent>(pageContent);

			return pageContent;
		}

		public void UpdatePageContent(PageContent content)
		{
			SaveOrUpdate<PageContent>(content);
		}

		public void Dispose()
		{

		}
	}
}
