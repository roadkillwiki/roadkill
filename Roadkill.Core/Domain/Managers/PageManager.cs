using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using System.Xml.Serialization;
using System.IO;
using Roadkill.Core.Search;

namespace Roadkill.Core
{
	public class PageManager
	{
		private IQueryable<Page> Pages
		{
			get
			{
				return Page.Repository.Manager().Queryable<Page>();
			}
		}

		private IQueryable<PageContent> PageContents
		{
			get
			{
				return Page.Repository.Manager().Queryable<PageContent>();
			}
		}

		public PageSummary Get(int id)
		{
			Page page = Pages.FirstOrDefault(p => p.Id == id);

			if (page == null)
				return null;
			else
				return page.ToSummary();
		}

		public PageSummary FindByTitle(string title)
		{
			if (string.IsNullOrEmpty(title))
				return null;

			Page page = Pages.FirstOrDefault(p => p.Title.ToLower() == title.ToLower());

			if (page == null)
				return null;
			else
				return page.ToSummary();
		}

		public IEnumerable<PageSummary> AllPages()
		{
			IEnumerable<Page> pages = Pages.OrderBy(p => p.Title);
			IEnumerable<PageSummary> summaries = from page in pages 
												 select page.ToSummary();

			return summaries;
		}

		public IEnumerable<PageSummary> AllPagesCreatedBy(string userName)
		{
			IEnumerable<Page> pages = Pages.Where(p => p.CreatedBy == userName);
			IEnumerable<PageSummary> summaries = from page in pages
												 select page.ToSummary();

			return summaries;
		}

		public PageSummary AddPage(PageSummary summary)
		{
			string currentUser = RoadkillContext.Current.CurrentUser;

			Page page = new Page();
			page.Title = summary.Title;
			page.Tags = summary.Tags.CleanTags();
			page.CreatedBy = currentUser;
			page.CreatedOn = DateTime.Now;
			page.ModifiedOn = DateTime.Now;
			page.ModifiedBy = currentUser;
			Page.Repository.SaveOrUpdate(page);

			PageContent pageContent = new PageContent();
			pageContent.VersionNumber = 1;
			pageContent.Text = summary.Content;
			pageContent.EditedBy = currentUser;
			pageContent.EditedOn = DateTime.Now;
			pageContent.Page = page;
			PageContent.Repository.SaveOrUpdate(pageContent);

			// Update the lucene index
			SearchManager.Add(page);

			return page.ToSummary();
		}

		public void UpdatePage(PageSummary summary)
		{
			string currentUser = RoadkillContext.Current.CurrentUser;
			HistoryManager manager = new HistoryManager();

			Page page = Pages.FirstOrDefault(p => p.Id == summary.Id);
			page.Title = summary.Title;
			page.Tags = summary.Tags.CleanTags();
			page.ModifiedOn = DateTime.Now;
			page.ModifiedBy = currentUser;	
			Page.Repository.SaveOrUpdate(page);

			PageContent pageContent = new PageContent();
			pageContent.VersionNumber = manager.MaxVersion(summary.Id) + 1;
			pageContent.Text = summary.Content;
			pageContent.EditedBy = currentUser;
			pageContent.EditedOn = DateTime.Now;
			pageContent.Page = page;
			PageContent.Repository.SaveOrUpdate(pageContent);

			// Update the lucene index
			SearchManager.Update(page);
		}

		public IEnumerable<PageSummary> FindByTag(string tag)
		{
			IEnumerable<Page> pages = Pages.Where(p => p.Tags.Contains(tag)).OrderBy(p => p.Title);
			IEnumerable<PageSummary> summaries = from page in pages
												 select page.ToSummary();

			return summaries;
		}

		public IEnumerable<TagSummary> AllTags()
		{
			var tagList = from p in Pages select new { Tag = p.Tags };
			List<TagSummary> tags = new List<TagSummary>();
			foreach (var item in tagList)
			{
				string[] parts = item.Tag.Split(';');
				foreach (string part in parts)
				{
					if (!string.IsNullOrEmpty(part))
					{
						string tagName = part.ToLower();
						TagSummary summary = new TagSummary(tagName);
						int index = tags.IndexOf(summary);

						if (index < 0)
						{
							tags.Add(summary);
						}
						else
						{
							tags[index].Count++;	
						}
					}
				}
			}

			return tags;
		}

		public void DeletePage(int pageId)
		{
			// This isn't the "right" way to do it, but to avoid the pagecontent coming back
			// each time a page is requested, it has no inverse relationship.
			Page page = Pages.First(p => p.Id == pageId);

			IList<PageContent> children = PageContent.Repository.List("Page.Id", pageId);
			foreach (PageContent pageContent in children)
			{
				PageContent.Repository.Delete(pageContent);
			}

			Page.Repository.Delete(page);

			// Update the lucene index
			SearchManager.Delete(page);
		}

		public string ExportToXml()
		{
			IEnumerable<PageSummary> list = AllPages();

			XmlSerializer serializer = new XmlSerializer(typeof(List<PageSummary>));

			StringBuilder builder = new StringBuilder();
			using (StringWriter writer = new StringWriter(builder))
			{
				serializer.Serialize(writer, list);
				return builder.ToString();
			}
		}
	}
}
