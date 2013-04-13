using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Converters;

namespace Roadkill.Core.Database
{
	public interface IPageRepository
	{
		IEnumerable<Page> AllPages();
		Page GetPageById(int id);
		IEnumerable<Page> FindPagesCreatedBy(string username);
		IEnumerable<Page> FindPagesModifiedBy(string username);
		IEnumerable<Page> FindPagesContainingTag(string tag);

		/// <summary>
		/// Returns a list of tags for all pages. Each item is a list of tags seperated by ,
		/// e.g. { "tag1, tag2, tag3", "blah, blah2" } 
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> AllTags();

		/// <summary>
		/// Case insensitive search by page title
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		Page GetPageByTitle(string title);

		PageContent GetLatestPageContent(int pageId);
		PageContent GetPageContentById(Guid id);
		PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber);
		IEnumerable<PageContent> GetPageContentByEditedBy(string username);
		IEnumerable<PageContent> FindPageContentsByPageId(int pageId);
		IEnumerable<PageContent> FindPageContentsEditedBy(string username);
		IEnumerable<PageContent> AllPageContents();

		void DeletePage(Page page);
		/// <summary>
		/// Removes a single version of page contents by its id.
		/// </summary>
		/// <param name="pageContent"></param>
		void DeletePageContent(PageContent pageContent);
		void DeleteAllPages();
		void SaveOrUpdatePage(Page page);
		PageContent AddNewPage(Page page, string text, string editedBy, DateTime editedOn);
		PageContent AddNewPageContentVersion(Page page, string text, string editedBy, DateTime editedOn, int version);
		void UpdatePageContent(PageContent content); // no new version
	}
}
