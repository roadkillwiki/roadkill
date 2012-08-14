using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Linq;
using System.Xml.Serialization;
using System.IO;
using Roadkill.Core.Search;
using System.Web;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides a set of tasks for wiki page management.
	/// </summary>
	public class PageManager : ManagerBase
	{
		/// <summary>
		/// Adds the page to the database.
		/// </summary>
		/// <param name="summary">The summary details for the page.</param>
		/// <returns>A <see cref="PageSummary"/> for the newly added page.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while saving.</exception>
		/// <exception cref="SearchException">An error occurred adding the page to the search index.</exception>
		public PageSummary AddPage(PageSummary summary)
		{
			try
			{
				string currentUser = RoadkillContext.Current.CurrentUsername;

				Page page = new Page();
				page.Title = summary.Title;
				page.Tags = summary.Tags.CleanTags();
				page.CreatedBy = AppendIpForAppHarbor(currentUser);
				page.CreatedOn = DateTime.Now;
				page.ModifiedOn = DateTime.Now;
				page.ModifiedBy = AppendIpForAppHarbor(currentUser);
				NHibernateRepository.Current.SaveOrUpdate<Page>(page);

				PageContent pageContent = new PageContent();
				pageContent.VersionNumber = 1;
				pageContent.Text = summary.Content;
				pageContent.EditedBy = AppendIpForAppHarbor(currentUser);
				pageContent.EditedOn = DateTime.Now;
				pageContent.Page = page;
				NHibernateRepository.Current.SaveOrUpdate<PageContent>(pageContent);

				// Update the lucene index
				try
				{
					SearchManager.Current.Add(page.ToSummary());
				}
				catch (SearchException)
				{
					// TODO: log
				}

				return page.ToSummary();
			}
			catch (HibernateException e)
			{
				throw new DatabaseException(e, "An error occurred while adding page '{0}' to the database", summary.Title);
			}
		}

		/// <summary>
		/// Retrieves a list of all pages in the system.
		/// </summary>
		/// <returns>An <see cref="IEnumerable`PageSummary"/> of the pages.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while retrieving the list.</exception>
		public IEnumerable<PageSummary> AllPages()
		{
			try
			{
				IEnumerable<Page> pages = Pages.OrderBy(p => p.Title);
				IEnumerable<PageSummary> summaries = from page in pages
													 select page.ToSummary();

				return summaries;
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred while retrieving all pages from the database");
			}
		}

		/// <summary>
		/// Alls the pages created by.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns></returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while retrieving the list.</exception>
		public IEnumerable<PageSummary> AllPagesCreatedBy(string userName)
		{
			try
			{
				IEnumerable<Page> pages = Pages.Where(p => p.CreatedBy == userName);
				IEnumerable<PageSummary> summaries = from page in pages
													 select page.ToSummary();

				return summaries;
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred while retrieving all pages created by {0} from the database", userName);
			}
		}

		/// <summary>
		/// Retrieves a list of all tags in the system.
		/// </summary>
		/// <returns>A <see cref="IEnumerable`TagSummary`"/> for the tags.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the tags.</exception>
		public IEnumerable<TagSummary> AllTags()
		{
			try
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
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred while retrieving all tags from the database");
			}
		}

		/// <summary>
		/// Deletes a page from the database.
		/// </summary>
		/// <param name="pageId">The id of the page to remove.</param>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while deleting the page.</exception>
		public void DeletePage(int pageId)
		{
			try
			{
				// This isn't the "right" way to do it, but to avoid the pagecontent coming back
				// each time a page is requested, it has no inverse relationship.
				Page page = Pages.First(p => p.Id == pageId);

				// Update the lucene index before we actually delete the page.
				// We cannot call the ToSummary() method on an object that no longer exists.
				try
				{
					SearchManager.Current.Delete(page.ToSummary());
				}
				catch (SearchException)
				{
					// TODO: log.
				}

				IList<PageContent> children = PageContents.Where(p => p.Page.Id == pageId).ToList();
				for (int i = 0; i < children.Count; i++)
				{
					NHibernateUtil.Initialize(children[i].Page); // force the proxy to hydrate
					NHibernateRepository.Current.Delete<PageContent>(children[i]);
				}

				NHibernateRepository.Current.Delete<Page>(page);
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred while deleting the page id {0} from the database", pageId);
			}
		}

		/// <summary>
		/// Exports all pages in the database, including content, to an XML format.
		/// </summary>
		/// <returns>An XML string.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the list.</exception>
		/// <exception cref="InvalidOperationException">An XML serialiation occurred exporting the page content.</exception>
		public string ExportToXml()
		{
			try
			{
				List<PageSummary> list = AllPages().ToList();

				XmlSerializer serializer = new XmlSerializer(typeof(List<PageSummary>));

				StringBuilder builder = new StringBuilder();
				using (StringWriter writer = new StringWriter(builder))
				{
					serializer.Serialize(writer, list);
					return builder.ToString();
				}
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "A database error occurred while exporting the pages to XML");
			}
		}

		/// <summary>
		/// Finds all pages with the given tag.
		/// </summary>
		/// <param name="tag">The tag to search for.</param>
		/// <returns>A <see cref="IEnumerable`PageSummary"/> of pages tagged with the provided tag.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the list.</exception>
		public IEnumerable<PageSummary> FindByTag(string tag)
		{
			try
			{
				IEnumerable<Page> pages = Pages.Where(p => p.Tags.Contains(tag)).OrderBy(p => p.Title);
				IEnumerable<PageSummary> summaries = from page in pages
													 select page.ToSummary();

				return summaries;
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred finding the tag '{0}' in the database", tag);
			}
		}

		/// <summary>
		/// Finds a page by its title
		/// </summary>
		/// <param name="title">The page title</param>
		/// <returns>A <see cref="PageSummary"/> for the page.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the page.</exception>
		public PageSummary FindByTitle(string title)
		{
			try
			{
				if (string.IsNullOrEmpty(title))
					return null;

				Page page = Pages.FirstOrDefault(p => p.Title.ToLower() == title.ToLower());

				if (page == null)
					return null;
				else
					return page.ToSummary();
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred finding the page with title '{0}' in the database", title);
			}
		}

		/// <summary>
		/// Retrieves the page by its id.
		/// </summary>
		/// <param name="id">The id of the page</param>
		/// <returns>A <see cref="PageSummary"/> for the page.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the page.</exception>
		public PageSummary Get(int id)
		{
			try
			{
				Page page = Pages.FirstOrDefault(p => p.Id == id);

				if (page == null)
					return null;
				else
					return page.ToSummary();
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred getting the page with id '{0}' from the database", id);
			}
		}

		/// <summary>
		/// Updates the provided page.
		/// </summary>
		/// <param name="summary">The summary.</param>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while updating.</exception>
		/// <exception cref="SearchException">An error occurred adding the page to the search index.</exception>
		public void UpdatePage(PageSummary summary)
		{
			try
			{
				string currentUser = RoadkillContext.Current.CurrentUsername;
				HistoryManager manager = new HistoryManager();

				Page page = Pages.FirstOrDefault(p => p.Id == summary.Id);
				page.Title = summary.Title;
				page.Tags = summary.Tags.CleanTags();
				page.ModifiedOn = DateTime.Now;
				page.ModifiedBy = AppendIpForAppHarbor(currentUser);

				// A second check to ensure a fake IsLocked POST doesn't work.
				if (RoadkillContext.Current.IsAdmin)
					page.IsLocked = summary.IsLocked;

				NHibernateRepository.Current.SaveOrUpdate<Page>(page);

				PageContent pageContent = new PageContent();
				pageContent.VersionNumber = manager.MaxVersion(summary.Id) + 1;
				pageContent.Text = summary.Content;
				pageContent.EditedBy = AppendIpForAppHarbor(currentUser);
				pageContent.EditedOn = DateTime.Now;
				pageContent.Page = page;
				NHibernateRepository.Current.SaveOrUpdate<PageContent>(pageContent);

				// Update all links to this page, if it has had its title renamed
				//MarkupConverter converter = new MarkupConverter();
				//pageContent.Text = converter.ReplacePageLinks(pageContent.Text, "target page", "new name");

				// Update the lucene index
				SearchManager.Current.Update(page.ToSummary());
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred updating the page with title '{0}' in the database", summary.Title);
			}
		}

		/// <summary>
		/// Renames a tag by changing all pages that reference the tag to use the new tag name.
		/// </summary>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while saving one of the pages.</exception>
		/// <exception cref="SearchException">An error occurred updating the search index.</exception>
		public void RenameTag(string oldTagName, string newTagName)
		{
			try
			{
				IEnumerable<PageSummary> pageSummaries = FindByTag(oldTagName);

				foreach (PageSummary summary in pageSummaries)
				{
					SearchManager.Current.Delete(summary);

					summary.Tags = summary.Tags.Replace(oldTagName + ";", newTagName + ";");
					UpdatePage(summary);
				}
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred while changing the tagname {0} to {1}", oldTagName, newTagName);
			}
		}

		/// <summary>
		/// Adds an IP address after the username for any Appharbor vandalism.
		/// </summary>
		private string AppendIpForAppHarbor(string username)
		{
			string result = username;

#if APPHARBOR
			if (!RoadkillContext.Current.IsAdmin)
			{
				string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
				if (string.IsNullOrEmpty(ip))
					ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

				result = string.Format("{0} ({1})", username, ip);
			}
#endif

			return result;
		}
	}
}
