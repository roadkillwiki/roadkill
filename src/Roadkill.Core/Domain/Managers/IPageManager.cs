using System;
using System.Collections.Generic;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Managers
{
	public interface IPageManager
	{
		/// <summary>
		/// Adds the page to the database.
		/// </summary>
		/// <param name="summary">The summary details for the page.</param>
		/// <returns>A <see cref="PageSummary"/> for the newly added page.</returns>
		/// <exception cref="DatabaseException">An database error occurred while saving.</exception>
		/// <exception cref="SearchException">An error occurred adding the page to the search index.</exception>
		PageSummary AddPage(PageSummary summary);

		/// <summary>
		/// Retrieves a list of all pages in the system.
		/// </summary>
		/// <returns>An <see cref="IEnumerable`PageSummary"/> of the pages.</returns>
		/// <exception cref="DatabaseException">An database error occurred while retrieving the list.</exception>
		IEnumerable<PageSummary> AllPages(bool loadPageContent = false);

		/// <summary>
		/// Gets alls the pages created by a user.
		/// </summary>
		/// <param name="userName">Name of the user.</param>
		/// <returns>All pages created by the provided user, or an empty list if none are found.</returns>
		/// <exception cref="DatabaseException">An database error occurred while saving.</exception>
		IEnumerable<PageSummary> AllPagesCreatedBy(string userName);

		/// <summary>
		/// Retrieves a list of all tags in the system.
		/// </summary>
		/// <returns>A <see cref="IEnumerable{TagSummary}"/> for the tags.</returns>
		/// <exception cref="DatabaseException">An database error occurred while getting the tags.</exception>
		IEnumerable<TagSummary> AllTags();

		/// <summary>
		/// Deletes a page from the database.
		/// </summary>
		/// <param name="pageId">The id of the page to remove.</param>
		/// <exception cref="DatabaseException">An database error occurred while deleting the page.</exception>
		void DeletePage(int pageId);

		/// <summary>
		/// Exports all pages in the database, including content, to an XML format.
		/// </summary>
		/// <returns>An XML string.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the list.</exception>
		/// <exception cref="InvalidOperationException">An XML serialiation occurred exporting the page content.</exception>
		string ExportToXml();

		/// <summary>
		/// Finds all pages with the given tag.
		/// </summary>
		/// <param name="tag">The tag to search for.</param>
		/// <returns>A <see cref="IEnumerable{PageSummary}"/> of pages tagged with the provided tag.</returns>
		/// <exception cref="DatabaseException">An database error occurred while getting the list.</exception>
		IEnumerable<PageSummary> FindByTag(string tag);

		/// <summary>
		/// Finds the first page with the tag 'homepage'. Any pages that are locked by an administrator take precedence.
		/// </summary>
		/// <returns>The homepage.</returns>
		PageSummary FindHomePage();

		/// <summary>
		/// Finds a page by its title
		/// </summary>
		/// <param name="title">The page title</param>
		/// <returns>A <see cref="PageSummary"/> for the page.</returns>
		/// <exception cref="DatabaseException">An NHibernate (database) error occurred while getting the page.</exception>
		PageSummary FindByTitle(string title);

		/// <summary>
		/// Retrieves the page by its id.
		/// </summary>
		/// <param name="id">The id of the page</param>
		/// <returns>A <see cref="PageSummary"/> for the page.</returns>
		/// <exception cref="DatabaseException">An database error occurred while getting the page.</exception>
		PageSummary GetById(int id);

		/// <summary>
		/// Retrieves the current text content for a page.
		/// </summary>
		/// <param name="pageId">The id of the page.</param>
		/// <returns>The <see cref="PageContent"/> for the page.</returns>
		PageContent GetCurrentContent(int pageId);

		/// <summary>
		/// Retrieves the <see cref="MarkupConverter"/> used by this pagemanager.
		/// </summary>
		/// <returns></returns>
		Converters.MarkupConverter GetMarkupConverter();

		/// <summary>
		/// Renames a tag by changing all pages that reference the tag to use the new tag name.
		/// </summary>
		/// <exception cref="DatabaseException">An database error occurred while saving one of the pages.</exception>
		/// <exception cref="SearchException">An error occurred updating the search index.</exception>
		void RenameTag(string oldTagName, string newTagName);

		/// <summary>
		/// Updates all links in pages to another page, when that page's title is changed.
		/// </summary>
		/// <param name="oldTitle">The previous page title.</param>
		/// <param name="newTitle">The new page title.</param>
		void UpdateLinksToPage(string oldTitle, string newTitle);

		/// <summary>
		/// Updates the provided page.
		/// </summary>
		/// <param name="summary">The summary.</param>
		/// <exception cref="DatabaseException">An database error occurred while updating.</exception>
		/// <exception cref="SearchException">An error occurred adding the page to the search index.</exception>
		void UpdatePage(PageSummary summary);
	}
}
