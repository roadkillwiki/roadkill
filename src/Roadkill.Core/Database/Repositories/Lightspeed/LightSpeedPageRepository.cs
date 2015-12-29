using System;
using System.Collections.Generic;
using System.Linq;
using Mindscape.LightSpeed;
using Mindscape.LightSpeed.Linq;
using Mindscape.LightSpeed.Querying;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Database.LightSpeed
{
	public class LightSpeedPageRepository : IPageRepository
	{
		internal readonly IUnitOfWork _unitOfWork;

		internal IQueryable<PageEntity> Pages => UnitOfWork.Query<PageEntity>();
		internal IQueryable<PageContentEntity> PageContents => UnitOfWork.Query<PageContentEntity>();

		public IUnitOfWork UnitOfWork
		{
			get
			{
				if (_unitOfWork == null)
				{
                    throw new DatabaseException("The IUnitOfWork for Lightspeed is null", null);
				}

				return _unitOfWork;
			}
		}

		public LightSpeedPageRepository(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn)
		{
			PageEntity pageEntity = new PageEntity();
			ToEntity.FromPage(page, pageEntity);
			pageEntity.Id = 0;
			UnitOfWork.Add(pageEntity);
			UnitOfWork.SaveChanges();

			PageContentEntity pageContentEntity = new PageContentEntity()
			{
				Id = Guid.NewGuid(),
				Page = pageEntity,
				Text = text,
				EditedBy = editedBy,
				EditedOn = editedOn,
				VersionNumber = 1,
			};

			UnitOfWork.Add(pageContentEntity);
			UnitOfWork.SaveChanges();

			PageContent pageContent = FromEntity.ToPageContent(pageContentEntity);
			pageContent.Page = FromEntity.ToPage(pageEntity);
			return pageContent;
		}

		public PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version)
		{
			if (version < 1)
				version = 1;

			PageEntity pageEntity = UnitOfWork.FindById<PageEntity>(page.Id);
			if (pageEntity != null)
			{
				// Update the content
				PageContentEntity pageContentEntity = new PageContentEntity()
				{
					Id = Guid.NewGuid(),
					Page = pageEntity,
					Text = text,
					EditedBy = editedBy,
					EditedOn = editedOn,
					VersionNumber = version,
				};

				UnitOfWork.Add(pageContentEntity);
				UnitOfWork.SaveChanges();

				// The page modified fields
				pageEntity.ModifiedOn = editedOn;
				pageEntity.ModifiedBy = editedBy;
				UnitOfWork.SaveChanges();

				// Turn the content database entity back into a domain object
				PageContent pageContent = FromEntity.ToPageContent(pageContentEntity);
				pageContent.Page = FromEntity.ToPage(pageEntity);

				return pageContent;
			}

			Log.Error("Unable to update page content for page id {0} (not found)", page.Id);
			return null;
		}

		public IEnumerable<Page> AllPages()
		{
			List<PageEntity> entities = Pages.ToList();
			return FromEntity.ToPageList(entities);
		}

		public IEnumerable<PageContent> AllPageContents()
		{
			List<PageContentEntity> entities = PageContents.ToList();
			return FromEntity.ToPageContentList(entities);
		}

		public IEnumerable<string> AllTags()
		{
			return new List<string>(Pages.Select(p => p.Tags));
		}

		public void DeleteAllPages()
		{
			UnitOfWork.Remove(new Query(typeof(PageEntity)));
			UnitOfWork.SaveChanges();

			UnitOfWork.Remove(new Query(typeof(PageContentEntity)));
			UnitOfWork.SaveChanges();
		}

		public void DeletePage(Page page)
		{
			PageEntity entity = UnitOfWork.FindById<PageEntity>(page.Id);
			UnitOfWork.Remove(entity);
			UnitOfWork.SaveChanges();
		}

		public void DeletePageContent(PageContent pageContent)
		{
			PageContentEntity entity = UnitOfWork.FindById<PageContentEntity>(pageContent.Id);
			UnitOfWork.Remove(entity);
			UnitOfWork.SaveChanges();
		}

		public IEnumerable<Page> FindPagesCreatedBy(string username)
		{
			List<PageEntity> entities = Pages.Where(p => p.CreatedBy == username).ToList();
			return FromEntity.ToPageList(entities);
		}

		public IEnumerable<Page> FindPagesModifiedBy(string username)
		{
			List<PageEntity> entities = Pages.Where(p => p.ModifiedBy == username).ToList();
			return FromEntity.ToPageList(entities);
		}

		public IEnumerable<Page> FindPagesContainingTag(string tag)
		{
			IEnumerable<PageEntity> entities = Pages.Where(p => p.Tags.ToLower().Contains(tag.ToLower())); // Lightspeed doesn't support ToLowerInvariant
			return FromEntity.ToPageList(entities);
		}

		public IEnumerable<PageContent> FindPageContentsByPageId(int pageId)
		{
			List<PageContentEntity> entities = PageContents.Where(p => p.Page.Id == pageId).ToList();
			return FromEntity.ToPageContentList(entities);
		}

		public IEnumerable<PageContent> FindPageContentsEditedBy(string username)
		{
			List<PageContentEntity> entities = PageContents.Where(p => p.EditedBy == username).ToList();
			return FromEntity.ToPageContentList(entities);
		}

		public Page GetPageById(int id)
		{
			PageEntity entity = Pages.FirstOrDefault(p => p.Id == id);
			return FromEntity.ToPage(entity);
		}

		public Page GetPageByTitle(string title)
		{
			PageEntity entity = Pages.FirstOrDefault(p => p.Title.ToLower() == title.ToLower());
			return FromEntity.ToPage(entity);
		}

		public PageContent GetLatestPageContent(int pageId)
		{
			PageContentEntity entity = PageContents.Where(x => x.Page.Id == pageId).OrderByDescending(x => x.EditedOn).FirstOrDefault();
			return FromEntity.ToPageContent(entity);
		}

		public PageContent GetPageContentById(Guid id)
		{
			PageContentEntity entity = PageContents.FirstOrDefault(p => p.Id == id);
			return FromEntity.ToPageContent(entity);
		}

		public PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber)
		{
			PageContentEntity entity = PageContents.FirstOrDefault(p => p.Page.Id == id && p.VersionNumber == versionNumber);
			return FromEntity.ToPageContent(entity);
		}

		public IEnumerable<PageContent> GetPageContentByEditedBy(string username)
		{
			List<PageContentEntity> entities = PageContents.Where(p => p.EditedBy == username).ToList();
			return FromEntity.ToPageContentList(entities);
		}

		public Page SaveOrUpdatePage(Page page)
		{
			PageEntity entity = UnitOfWork.FindById<PageEntity>(page.Id);
			if (entity == null)
			{
				entity = new PageEntity();
				ToEntity.FromPage(page, entity);
				UnitOfWork.Add(entity);
				UnitOfWork.SaveChanges();
				page = FromEntity.ToPage(entity);
			}
			else
			{
				ToEntity.FromPage(page, entity);
				UnitOfWork.SaveChanges();
				page = FromEntity.ToPage(entity);
			}

			return page;
		}

		/// <summary>
		/// This updates an existing set of text and is used for page rename updates.
		/// To add a new version of a page, use AddNewPageContentVersion
		/// </summary>
		/// <param name="content"></param>
		public void UpdatePageContent(PageContent content)
		{
			PageContentEntity entity = UnitOfWork.FindById<PageContentEntity>(content.Id);
			if (entity != null)
			{
				ToEntity.FromPageContent(content, entity);
				UnitOfWork.SaveChanges();
				content = FromEntity.ToPageContent(entity);
			}
		}

		public void Dispose()
		{
			_unitOfWork.SaveChanges();
			_unitOfWork.Dispose();
		}
	}
}
