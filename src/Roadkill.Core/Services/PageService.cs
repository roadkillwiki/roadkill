using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Cache;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Configuration;
using System.Web;
using Roadkill.Core.Logging;
using Roadkill.Core.Text;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Services
{
	/// <summary>
	/// Provides a set of tasks for wiki page management.
	/// </summary>
	public class PageService : ServiceBase, IPageService
	{
		private SearchService _searchService;
		private MarkupConverter _markupConverter;
		private PageHistoryService _historyService;
		private IUserContext _context;
		private ListCache _listCache;
		private PageViewModelCache _pageViewModelCache;
		private SiteCache _siteCache;
		private IPluginFactory _pluginFactory;
		private MarkupLinkUpdater _markupLinkUpdater;

		public PageService(ApplicationSettings settings, IRepository repository, SearchService searchService, 
			PageHistoryService historyService, IUserContext context, 
			ListCache listCache, PageViewModelCache pageViewModelCache, SiteCache sitecache, IPluginFactory pluginFactory)
			: base(settings, repository)
		{
			_searchService = searchService;
			_markupConverter = new MarkupConverter(settings, repository, pluginFactory);
			_historyService = historyService;
			_context = context;
			_listCache = listCache;
			_pageViewModelCache = pageViewModelCache;
			_siteCache = sitecache;
			_pluginFactory = pluginFactory;
			_markupLinkUpdater = new MarkupLinkUpdater(_markupConverter.Parser);
		}

		/// <summary>
		/// Adds the page to the database.
		/// </summary>
		/// <param name="model">The summary details for the page.</param>
		/// <returns>A <see cref="PageViewModel"/> for the newly added page.</returns>
		/// <exception cref="DatabaseException">An databaseerror occurred while saving.</exception>
		/// <exception cref="SearchException">An error occurred adding the page to the search index.</exception>
		public PageViewModel AddPage(PageViewModel model)
		{
			try
			{
				string currentUser = _context.CurrentUsername;

				Page page = new Page();
				page.Title = model.Title;
				page.Tags = model.CommaDelimitedTags();
				page.CreatedBy = AppendIpForDemoSite(currentUser);
				page.CreatedOn = DateTime.UtcNow;
				page.ModifiedOn = DateTime.UtcNow;
				page.ModifiedBy = AppendIpForDemoSite(currentUser);

				// Double check, incase the HTML form was faked.
				if (_context.IsAdmin)
					page.IsLocked = model.IsLocked;

				PageContent pageContent = Repository.AddNewPage(page, model.Content, AppendIpForDemoSite(currentUser), DateTime.UtcNow);

				_listCache.RemoveAll();
				_pageViewModelCache.RemoveAll(); // completely clear the cache to update any reciprocal links.

				// Update the lucene index
				PageViewModel savedModel = new PageViewModel(pageContent, _markupConverter);
				try
				{
					_searchService.Add(savedModel);
				}
				catch (SearchException)
				{
					// TODO: log
				}

				return savedModel;
			}
			catch (DatabaseException e)
			{
				throw new DatabaseException(e, "An error occurred while adding page '{0}' to the database", model.Title);
			}
		}

		/// <summary>
		/// Retrieves a list of all pages in the system.
		/// </summary>
		/// <returns>An <see cref="IEnumerable{PageViewModel}"/> of the pages.</returns>
		/// <exception cref="DatabaseException">An databaseerror occurred while retrieving the list.</exception>
		public IEnumerable<PageViewModel> AllPages(bool loadPageContent = false)
		{
			try
			{
				string cacheKey = "";
				IEnumerable<PageViewModel> pageModels;

				if (loadPageContent)
				{
					cacheKey = CacheKeys.AllPagesWithContent();
					pageModels = _listCache.Get<PageViewModel>(cacheKey);

					if (pageModels == null)
					{
						IEnumerable<Page> pages = Repository.AllPages().OrderBy(p => p.Title);
						pageModels = from page in pages
									select new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);

						_listCache.Add<PageViewModel>(cacheKey, pageModels);
					}
				}
				else
				{
					cacheKey = CacheKeys.AllPages();
					pageModels = _listCache.Get<PageViewModel>(cacheKey);

					if (pageModels == null)
					{
						IEnumerable<Page> pages = Repository.AllPages().OrderBy(p => p.Title);
						pageModels = from page in pages
									select new PageViewModel() { Id = page.Id, Title = page.Title };

						_listCache.Add<PageViewModel>(cacheKey, pageModels);
					}
				}

				return pageModels;
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred while retrieving all pages from the database");
			}
		}

		/// <summary>
		/// Gets alls the pages created by a user.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>All pages created by the provided user, or an empty list if none are found.</returns>
		/// <exception cref="DatabaseException">An databaseerror occurred while retrieving the list.</exception>
		public IEnumerable<PageViewModel> AllPagesCreatedBy(string userName)
		{
			try
			{
				string cacheKey = string.Format("allpages.createdby.{0}", userName);

				IEnumerable<PageViewModel> models = _listCache.Get<PageViewModel>(cacheKey);
				if (models == null)
				{
					IEnumerable<Page> pages = Repository.FindPagesCreatedBy(userName);
					models = from page in pages
								select new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);

					_listCache.Add<PageViewModel>(cacheKey, models);
				}

				return models;
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred while retrieving all pages created by {0} from the database", userName);
			}
		}

		/// <summary>
		/// Retrieves a list of all tags in the system.
		/// </summary>
		/// <returns>A <see cref="IEnumerable{TagViewModel}"/> for the tags.</returns>
		/// <exception cref="DatabaseException">An databaseerror occurred while getting the tags.</exception>
		public IEnumerable<TagViewModel> AllTags()
		{
			try
			{
				string cacheKey = "alltags";

				List<TagViewModel> tags = _listCache.Get<TagViewModel>(cacheKey);
				if (tags == null)
				{
					IEnumerable<string> tagList = Repository.AllTags();
					tags = new List<TagViewModel>();

					foreach (string item in tagList)
					{
						foreach (string tagName in PageViewModel.ParseTags(item))
						{
							if (!string.IsNullOrEmpty(tagName))
							{
								TagViewModel tagModel = new TagViewModel(tagName);
								int index = tags.IndexOf(tagModel);

								if (index < 0)
								{
									tags.Add(tagModel);
								}
								else
								{
									tags[index].Count++;
								}
							}
						}
					}

					_listCache.Add<TagViewModel>(cacheKey, tags);
				}

				return tags;
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred while retrieving all tags from the database");
			}
		}

		/// <summary>
		/// Deletes a page from the database.
		/// </summary>
		/// <param name="pageId">The id of the page to remove.</param>
		/// <exception cref="DatabaseException">An databaseerror occurred while deleting the page.</exception>
		public void DeletePage(int pageId)
		{
			try
			{
				// Avoid grabbing all the pagecontents coming back each time a page is requested, it has no inverse relationship.
				Page page = Repository.GetPageById(pageId);

				// Update the lucene index before we actually delete the page.
				try
				{
					PageViewModel model = new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);
					_searchService.Delete(model);
				}
				catch (SearchException ex)
				{
					Log.Error(ex, "Unable to delete page with id {0} from the lucene index", pageId);
				}

				IList<PageContent> children = Repository.FindPageContentsByPageId(pageId).ToList();
				for (int i = 0; i < children.Count; i++)
				{
					Repository.DeletePageContent(children[i]);
				}

				Repository.DeletePage(page);

				// Remove everything for now, to avoid reciprocal link issues
				_listCache.RemoveAll();
				_pageViewModelCache.RemoveAll();
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred while deleting the page id {0} from the database", pageId);
			}
		}

		/// <summary>
		/// Exports all pages in the database, including content, to an XML format.
		/// </summary>
		/// <returns>An XML string.</returns>
		/// <exception cref="DatabaseException">An databaseerror occurred while getting the list.</exception>
		/// <exception cref="InvalidOperationException">An XML serialiation occurred exporting the page content.</exception>
		public string ExportToXml()
		{
			try
			{
				List<PageViewModel> list = AllPages().ToList();

				XmlSerializer serializer = new XmlSerializer(typeof(List<PageViewModel>));

				StringBuilder builder = new StringBuilder();
				using (StringWriter writer = new StringWriter(builder))
				{
					serializer.Serialize(writer, list);
					return builder.ToString();
				}
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "A database error occurred while exporting the pages to XML");
			}
		}

		/// <summary>
		/// Finds the first page with the tag 'homepage'. Any pages that are locked by an administrator take precedence.
		/// </summary>
		/// <returns>The homepage.</returns>
		public PageViewModel FindHomePage()
		{
			try
			{
				PageViewModel pageModel = _pageViewModelCache.GetHomePage();
				if (pageModel == null)
				{

					Page page = Repository.FindPagesContainingTag("homepage").FirstOrDefault(x => x.IsLocked == true);
					if (page == null)
					{
						page = Repository.FindPagesContainingTag("homepage").FirstOrDefault();
					}
					
					if (page != null)
					{
						pageModel = new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);
						_pageViewModelCache.UpdateHomePage(pageModel);
					}
				}

				return pageModel;
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred finding the tag 'homepage' in the database");
			}
		}

		/// <summary>
		/// Finds all pages with the given tag.
		/// </summary>
		/// <param name="tag">The tag to search for.</param>
		/// <returns>A <see cref="IEnumerable{PageViewModel}"/> of pages tagged with the provided tag.</returns>
		/// <exception cref="DatabaseException">An database error occurred while getting the list.</exception>
		public IEnumerable<PageViewModel> FindByTag(string tag)
		{
			try
			{
				string cacheKey = string.Format("pagesbytag.{0}", tag);

				IEnumerable<PageViewModel> models = _listCache.Get<PageViewModel>(cacheKey);
				if (models == null)
				{

					IEnumerable<Page> pages = Repository.FindPagesContainingTag(tag).OrderBy(p => p.Title);
					models = from page in pages
								select new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);

					_listCache.Add<PageViewModel>(cacheKey, models);
				}

				return models;
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred finding the tag '{0}' in the database", tag);
			}
		}

		/// <summary>
		/// Finds a page by its title
		/// </summary>
		/// <param name="title">The page title</param>
		/// <returns>A <see cref="PageViewModel"/> for the page.</returns>
		/// <exception cref="DatabaseException">An databaseerror occurred while getting the page.</exception>
		public PageViewModel FindByTitle(string title)
		{
			try
			{
				if (string.IsNullOrEmpty(title))
					return null;

				Page page = Repository.GetPageByTitle(title);

				if (page == null)
					return null;
				else
					return new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred finding the page with title '{0}' in the database", title);
			}
		}

		/// <summary>
		/// Retrieves the page by its id.
		/// </summary>
		/// <param name="id">The id of the page</param>
		/// <returns>A <see cref="PageViewModel"/> for the page.</returns>
		/// <exception cref="DatabaseException">An databaseerror occurred while getting the page.</exception>
		public PageViewModel GetById(int id, bool loadContent = false)
		{
			try
			{
				PageViewModel pageModel = _pageViewModelCache.Get(id);
				if (pageModel != null)
				{
					return pageModel;
				}
				else
				{
					Page page = Repository.GetPageById(id);

					if (page == null)
					{
						return null;
					}
					else
					{
						// If object caching is enabled, ignore the "loadcontent" parameter as the cache will be 
						// used on the second call anyway, so performance isn't an issue.
						if (ApplicationSettings.UseObjectCache)
						{
							pageModel = new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);
						}
						else
						{
							if (loadContent)
							{
								pageModel = new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);
							}
							else
							{
								pageModel = new PageViewModel(page);
							}
						}

						_pageViewModelCache.Add(id, pageModel);
						return pageModel;
					}
				}
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred getting the page with id '{0}' from the database", id);
			}
		}

		/// <summary>
		/// Updates the provided page.
		/// </summary>
		/// <param name="model">The summary.</param>
		/// <exception cref="DatabaseException">An databaseerror occurred while updating.</exception>
		/// <exception cref="SearchException">An error occurred adding the page to the search index.</exception>
		public void UpdatePage(PageViewModel model)
		{
			try
			{
				string currentUser = _context.CurrentUsername;

				Page page = Repository.GetPageById(model.Id);
				page.Title = model.Title;
				page.Tags = model.CommaDelimitedTags();
				page.ModifiedOn = DateTime.UtcNow;
				page.ModifiedBy = AppendIpForDemoSite(currentUser);

				// A second check to ensure a fake IsLocked POST doesn't work.
				if (_context.IsAdmin)
					page.IsLocked = model.IsLocked;

				Repository.SaveOrUpdatePage(page);

				//
				// Update the cache - updating a page is expensive for the cache right now
				// this could be improved by updating the item in the listcache instead of invalidating it
				//
				_pageViewModelCache.Remove(model.Id , 0);

				if (model.Tags.Contains("homepage"))
					_pageViewModelCache.RemoveHomePage();

				_listCache.RemoveAll();

				int newVersion = _historyService.MaxVersion(model.Id) + 1;
				PageContent pageContent = Repository.AddNewPageContentVersion(page, model.Content, AppendIpForDemoSite(currentUser), DateTime.UtcNow, newVersion); 

				// Update all links to this page (if it has had its title renamed). Case changes don't need any updates.
				if (model.PreviousTitle != null && model.PreviousTitle.ToLower() != model.Title.ToLower())
				{
					UpdateLinksToPage(model.PreviousTitle, model.Title);
				}

				// Update the lucene index
				PageViewModel updatedModel = new PageViewModel(Repository.GetLatestPageContent(page.Id), _markupConverter);
				_searchService.Update(updatedModel);
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An error occurred updating the page with title '{0}' in the database", model.Title);
			}
		}

		/// <summary>
		/// Renames a tag by changing all pages that reference the tag to use the new tag name.
		/// </summary>
		/// <exception cref="DatabaseException">An databaseerror occurred while saving one of the pages.</exception>
		/// <exception cref="SearchException">An error occurred updating the search index.</exception>
		public void RenameTag(string oldTagName, string newTagName)
		{
			if (string.IsNullOrEmpty(oldTagName) || string.IsNullOrEmpty(newTagName))
				return;

			try
			{
				IEnumerable<PageViewModel> pageModels = FindByTag(oldTagName);

				foreach (PageViewModel model in pageModels)
				{
					_searchService.Delete(model);

					string tags = model.CommaDelimitedTags();

					if (tags.IndexOf(",") != -1)
					{
						tags = tags.Replace(oldTagName + ",", newTagName + ",");
					}
					else if (tags.IndexOf(";") != -1)
					{
						// legacy
						tags = tags.Replace(oldTagName + ";", newTagName + ";");
					}
					else
					{
						// Single tag
						tags = tags.Replace(oldTagName, newTagName);
					}

					model.RawTags = tags;
					UpdatePage(model);
				}

				string cacheKey = CacheKeys.PagesByTagKey(oldTagName);
				_listCache.Remove(cacheKey);
			}
			catch (DatabaseException ex)
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
		/// Adds an IP address after the username for any demo site vandalism.
		/// </summary>
		private string AppendIpForDemoSite(string username)
		{
			string result = username;

			if (ApplicationSettings.IsDemoSite)
			{
				if (!_context.IsAdmin)
				{
					string ip = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
					if (string.IsNullOrEmpty(ip))
						ip = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

					result = string.Format("{0} ({1})", username, ip);
				}
			}

			return result;
		}

		/// <summary>
		/// Updates all links in pages to another page, when that page's title is changed.
		/// </summary>
		/// <param name="oldTitle">The previous page title.</param>
		/// <param name="newTitle">The new page title.</param>
		public void UpdateLinksToPage(string oldTitle, string newTitle)
		{
			bool shouldClearCache = false;

			foreach (PageContent content in Repository.AllPageContents())
			{
				if (_markupLinkUpdater.ContainsPageLink(content.Text, oldTitle))
				{
					content.Text = _markupLinkUpdater.ReplacePageLinks(content.Text, oldTitle, newTitle);
					Repository.UpdatePageContent(content);
					
					shouldClearCache = true;
				}
			}

			if (shouldClearCache)
			{
				_pageViewModelCache.RemoveAll();
				_listCache.RemoveAll();
			}
		}

		/// <summary>
		/// Retrieves the (usually left) menu containing the new page, settings etc. options
		/// </summary>
		public string GetMenu(IUserContext userContext)
		{
			MenuParser parser = new MenuParser(_markupConverter, Repository, _siteCache, userContext);

			// TODO: turn this into a theme-based bit of template HTML
			StringBuilder builder = new StringBuilder();

			builder.AppendLine("<div id=\"leftmenu\">");
			builder.AppendLine(parser.GetMenu());
			builder.AppendLine("</div>");

			return builder.ToString();
		}

		/// <summary>
		/// Retrieves the (usually left) menu containing the new page, settings etc. options
		/// </summary>
		public string GetBootStrapNavMenu(IUserContext userContext)
		{
			MenuParser parser = new MenuParser(_markupConverter, Repository, _siteCache, userContext);

			// TODO: turn this into a theme-based bit of template HTML
			StringBuilder builder = new StringBuilder();

			builder.AppendLine("<nav id=\"leftmenu\" class=\"navbar navbar-default\" role=\"navigation\">");
			builder.Append(GetCollapsableMenuHtml());

			builder.AppendLine(@"<div id=""left-menu-toggle"" class=""collapse navbar-collapse"">");

			// Add bootstrap into the <ul>
			string menuHtml = parser.GetMenu();
			menuHtml = menuHtml.Replace("<ul>", "<ul class =\"nav navbar-nav\">");
			builder.AppendLine(menuHtml);

			builder.AppendLine("</div>");
			builder.AppendLine("</nav>");

			return builder.ToString();
		}

		/// <summary>
		/// Adds the Adidas bar to the nav bar so it can be collapsed on mobile devices
		/// </summary>
		/// <returns></returns>
		private string GetCollapsableMenuHtml()
		{
			string html = @"<div class=""navbar-header"">
					<button type=""button"" class=""navbar-toggle"" data-toggle=""collapse"" data-target=""#left-menu-toggle"">
						<span class=""sr-only"">Toggle navigation</span>
						<span class=""icon-bar""></span>
						<span class=""icon-bar""></span>
						<span class=""icon-bar""></span>
					</button>
				</div>";

			return html;
		}

		/// <summary>
		/// Retrieves the <see cref="MarkupConverter"/> used by this IPageService.
		/// </summary>
		/// <returns></returns>
		public MarkupConverter GetMarkupConverter()
		{
			return new MarkupConverter(ApplicationSettings, Repository, _pluginFactory);
		}

		/// <summary>
		/// Clears all pages and page content from the database.
		/// </summary>
		/// <exception cref="DatabaseException">An datastore error occurred while clearing the page data.</exception>
		public void ClearPageTables()
		{
			try
			{
				Repository.DeleteAllPages();
			}
			catch (DatabaseException ex)
			{
				throw new DatabaseException(ex, "An exception occurred while clearing all page tables.");
			}
		}
	}
}
