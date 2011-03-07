using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using System.Xml.Serialization;
using System.IO;

namespace Roadkill.Core
{
	public class PageManager
	{
		private static UserManager _userManager;

		public static UserManager Current
		{
			get
			{
				if (_userManager == null)
					_userManager = new UserManager();

				return _userManager;
			}
		}

		public PageSummary GetPage(string title)
		{
			IQuery query = PageContent.Repository.Manager().SessionFactory.OpenSession()
				.CreateQuery("FROM Page WHERE lower(Title) = :title");

			title = title.ToLower();
			query.SetString("title",title);
			query.SetMaxResults(1);
			Page page = query.UniqueResult<Page>();

			if (page == null)
				return null;
			else
				return page.ToSummary();
		}

		public PageSummary Get(Guid id)
		{
			Page page = Page.Repository.Read(id);
			if (page == null)
				return null;
			else
				return page.ToSummary();
		}

		public IEnumerable<PageSummary> AllPages()
		{
			IList<Page> pages = Page.Repository.List();
			List<PageSummary> list = new List<PageSummary>();
			foreach (Page page in pages)
			{
				list.Add(page.ToSummary());
			}

			return list.OrderBy(p => p.Title).ToList();
		}

		public IEnumerable<PageSummary> AllPagesCreatedBy(string userName)
		{
			IList<Page> pages = Page.Repository.List("CreatedBy", userName);
			List<PageSummary> list = new List<PageSummary>();
			foreach (Page page in pages)
			{
				list.Add(page.ToSummary());
			}

			return list.OrderBy(p => p.Title).ToList();
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

			return page.ToSummary();
		}

		public void UpdatePage(PageSummary summary)
		{
			string currentUser = RoadkillContext.Current.CurrentUser;
			HistoryManager manager = new HistoryManager();

			Page page = Page.Repository.Read(summary.Id);
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
		}

		public IEnumerable<PageSummary> FindByTag(string tag)
		{
			IQuery query = PageContent.Repository.Manager().SessionFactory.OpenSession()
				.CreateQuery("FROM Page WHERE Tags LIKE :tag");

			query.SetString("tag", "%" +tag+ ";%");
			IList<Page> pages = query.List<Page>();
			List<PageSummary> list = new List<PageSummary>();

			foreach (Page page in pages)
			{
				list.Add(page.ToSummary());
			}

			return list.OrderBy(p => p.Title).ToList();
		}

		public IEnumerable<TagSummary> AllTags()
		{
			IQuery query = PageContent.Repository.Manager().SessionFactory.OpenSession()
				.CreateQuery("SELECT Tags FROM Page");

			IList<string> list = query.List<string>();
			List<TagSummary> tags = new List<TagSummary>();
			foreach (string line in list)
			{
				string[] parts = line.Split(';');
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

		public void DeletePage(Guid pageId)
		{
			// This isn't the "right" way to do it, but to avoid the pagecontent coming back
			// each time a page is requested, it has no inverse relationship.
			Page page = Page.Repository.Read(pageId);

			IList<PageContent> children = PageContent.Repository.List("Page.Id", pageId);
			foreach (PageContent pageContent in children)
			{
				PageContent.Repository.Delete(pageContent);
			}

			Page.Repository.Delete(page);
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
