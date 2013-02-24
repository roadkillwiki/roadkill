using System;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core.Converters;

namespace Roadkill.Core
{
	public interface IPageRepository
	{
		IEnumerable<Page> AllPages();
		Page GetPageById(int id);
		IEnumerable<Page> FindPagesByCreatedBy(string username);
		IEnumerable<Page> FindPagesByModifiedBy(string username);
		IEnumerable<Page> FindPagesByTag(string tag);

		/// <summary>
		/// Returns a list of tags for all pages. Each item is a list of tags seperated by ,
		/// e.g. { "tag1, tag2, tag3", "blah, blah2" } 
		/// </summary>
		/// <returns></returns>
		IEnumerable<string> AllTags();

		Page GetPageByTitle(string title);

		PageContent GetLatestPageContent(int pageId);
		PageContent GetPageContentById(Guid id);
		PageContent GetLatestPageContentVersion(Guid id);
		PageContent GetPageContentByPageIdAndVersionNumber(int id, int versionNumber);
		PageContent GetPageContentByVersionId(Guid versionId);
		PageContent GetPageContentByEditBy(string username);
		IEnumerable<PageContent> FindPageContentsByPageId(int pageId);
		IEnumerable<PageContent> AllPageContents();
	}
}
