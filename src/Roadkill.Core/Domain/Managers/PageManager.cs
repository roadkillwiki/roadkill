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
using Lucene.Net.Documents;
using System.Text.RegularExpressions;
using Roadkill.Core.Configuration;
using StructureMap;

namespace Roadkill.Core
{
	/// <summary>
	/// Provides a set of tasks for wiki page management.
	/// </summary>
	public class PageManager : ServiceBase, IPageManager
	{
		private SearchManager _searchManager;
		private MarkupConverter _markupConverter;
		private HistoryManager _historyManager;
		private IRoadkillContext _context;

		public PageManager(IConfigurationContainer configuration, IRepository repository, SearchManager searchManager, 
			HistoryManager historyManager, IRoadkillContext context)
			: base(configuration, repository)
		{
			_searchManager = searchManager;
			_markupConverter = new MarkupConverter(configuration, repository);
			_historyManager = historyManager;
			_context = context;
		}

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
				string currentUser = _context.CurrentUsername;

				Page page = new Page();
				page.Title = summary.Title;
				page.Tags = summary.CommaDelimitedTags();
				page.CreatedBy = AppendIpForDemoSite(currentUser);
				page.CreatedOn = DateTime.Now;
				page.ModifiedOn = DateTime.Now;
				page.ModifiedBy = AppendIpForDemoSite(currentUser);
				Repository.SaveOrUpdate<Page>(page);

				PageContent pageContent = new PageContent();
				pageContent.VersionNumber = 1;
				pageContent.Text = summary.Content;
				pageContent.EditedBy = AppendIpForDemoSite(currentUser);
				pageContent.EditedOn = DateTime.Now;
				pageContent.Page = page;
				Repository.SaveOrUpdate<PageContent>(pageContent);

				// Update the lucene index
				PageSummary savedSummary = pageContent.ToSummary(_markupConverter);
				try
				{
					_searchManager.Add(savedSummary);
				}
				catch (SearchException)
				{
					// TODO: log
				}

				return savedSummary;
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
				IEnumerable<Page> pages = Repository.AllPages().OrderBy(p => p.Title);
				IEnumerable<PageSummary> summaries = from page in pages
													 select Repository.GetLatestPageContent(page.Id).ToSummary(_markupConverter);

				return summaries;
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred while retrieving all pages from the database");
			}
		}

		/// <summary>
		/// Gets alls the pages created by a user.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>All pages created by the provided user, or an empty list if none are found.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while retrieving the list.</exception>
		public IEnumerable<PageSummary> AllPagesCreatedBy(string userName)
		{
			try
			{
				IEnumerable<Page> pages = Repository.FindPagesByCreatedBy(userName);
				IEnumerable<PageSummary> summaries = from page in pages
													 select Repository.GetLatestPageContent(page.Id).ToSummary(_markupConverter);

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
		/// <returns>A <see cref="IEnumerable{TagSummary}"/> for the tags.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the tags.</exception>
		public IEnumerable<TagSummary> AllTags()
		{
			try
			{
				IEnumerable<string> tagList = Repository.AllTags();
				List<TagSummary> tags = new List<TagSummary>();

				foreach (string item in tagList)
				{
					foreach (string tagName in item.ParseTags())
					{
						if (!string.IsNullOrEmpty(tagName))
						{
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
				Page page = Repository.GetPageById(pageId);

				// Update the lucene index before we actually delete the page.
				// We cannot call the ToSummary() method on an object that no longer exists.
				try
				{
					_searchManager.Delete(Repository.GetLatestPageContent(page.Id).ToSummary(_markupConverter));
				}
				catch (SearchException)
				{
					// TODO: log.
				}

				IList<PageContent> children = Repository.FindPageContentsByPageId(pageId).ToList();
				for (int i = 0; i < children.Count; i++)
				{
					if (Repository is NHibernateRepository)
					{
						NHibernateUtil.Initialize(children[i].Page); // force the proxy to hydrate
					}

					Repository.Delete<PageContent>(children[i]);
				}

				Repository.Delete<Page>(page);
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
		/// <returns>A <see cref="IEnumerable{PageSummary}"/> of pages tagged with the provided tag.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the list.</exception>
		public IEnumerable<PageSummary> FindByTag(string tag)
		{
			try
			{
				IEnumerable<Page> pages = Repository.FindPagesContainingTag(tag).OrderBy(p => p.Title);
				IEnumerable<PageSummary> summaries = from page in pages
													 select Repository.GetLatestPageContent(page.Id).ToSummary(_markupConverter);

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

				Page page = Repository.GetPageByTitle(title);

				if (page == null)
					return null;
				else
					return Repository.GetLatestPageContent(page.Id).ToSummary(_markupConverter);
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
		public PageSummary GetById(int id)
		{
			try
			{
				Page page = Repository.GetPageById(id);

				if (page == null)
					return null;
				else
					return Repository.GetLatestPageContent(page.Id).ToSummary(_markupConverter);
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
				string currentUser = _context.CurrentUsername;

				Page page = Repository.GetPageById(summary.Id);
				page.Title = summary.Title;
				page.Tags = summary.CommaDelimitedTags();
				page.ModifiedOn = DateTime.Now;
				page.ModifiedBy = AppendIpForDemoSite(currentUser);

				// A second check to ensure a fake IsLocked POST doesn't work.
				if (_context.IsAdmin)
					page.IsLocked = summary.IsLocked;

				Repository.SaveOrUpdate<Page>(page);

				PageContent pageContent = new PageContent();
				pageContent.VersionNumber = _historyManager.MaxVersion(summary.Id) + 1;
				pageContent.Text = summary.Content;
				pageContent.EditedBy = AppendIpForDemoSite(currentUser);
				pageContent.EditedOn = DateTime.Now;
				pageContent.Page = page;
				Repository.SaveOrUpdate<PageContent>(pageContent);

				// Update all links to this page (if it has had its title renamed). Case changes don't need any updates.
				if (summary.PreviousTitle != null && summary.PreviousTitle.ToLower() != summary.Title.ToLower())
				{
					UpdateLinksToPage(summary.PreviousTitle, summary.Title);
				}

				// Update the lucene index
				_searchManager.Update(Repository.GetLatestPageContent(page.Id).ToSummary(_markupConverter));
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
					_searchManager.Delete(summary);

					string tags = summary.CommaDelimitedTags();

					if (tags.IndexOf(";") != -1)
					{
						tags = tags.Replace(oldTagName + ";", newTagName + ";");
					}
					else if (tags.IndexOf(",") != -1)
					{
						tags = tags.Replace(oldTagName + ",", newTagName + ",");
					}

					summary.RawTags = tags;
					UpdatePage(summary);
				}
			}
			catch (HibernateException ex)
			{
				throw new DatabaseException(ex, "An error occurred while changing the tagname {0} to {1}", oldTagName, newTagName);
			}
		}

		/// <summary>
		/// Retrieves the current text content for a page.
		/// </summary>
		/// <param name="pageId">The id of the page.</param>
		/// <returns>The <see cref="PageContent"/> for the page.</returns>
		public PageContent GetCurrentContent(int pageId)
		{
			return Repository.GetLatestPageContent(pageId);
		}

		/// <summary>
		/// Adds an IP address after the username for any Appharbor vandalism.
		/// </summary>
		private string AppendIpForDemoSite(string username)
		{
			string result = username;

#if DEMOSITE
			Console.WriteLine("NUnit warning: You're running using #DEMOSITE !");
			if (!_context.IsAdmin)
			{
				string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
				if (string.IsNullOrEmpty(ip))
					ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

				result = string.Format("{0} ({1})", username, ip);
			}
#endif

			return result;
		}

		/// <summary>
		/// Updates all links in pages to another page, when that page's title is changed.
		/// </summary>
		/// <param name="oldTitle">The previous page title.</param>
		/// <param name="newTitle">The new page title.</param>
		public void UpdateLinksToPage(string oldTitle, string newTitle)
		{
			foreach (PageContent content in Repository.AllPageContents())
			{
				if (_markupConverter.ContainsPageLink(content.Text, oldTitle))
				{
					// force the proxy to hydrate, for "Illegally attempted to associate a proxy with two open Sessions" errors
					if (Repository is NHibernateRepository)
					{
						NHibernateUtil.Initialize(content.Page);
					}

					content.Text = _markupConverter.ReplacePageLinks(content.Text, oldTitle, newTitle);
					Repository.SaveOrUpdate<PageContent>(content);
				}
			}
		}

		/// <summary>
		/// Retrieves the <see cref="MarkupConverter"/> used by this pagemanager.
		/// </summary>
		/// <returns></returns>
		public MarkupConverter GetMarkupConverter()
		{
			return new MarkupConverter(Configuration, Repository);
		}
	}
}
