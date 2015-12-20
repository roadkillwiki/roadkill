using System.Collections.Generic;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Services
{
	public interface ISearchService
	{
		ApplicationSettings ApplicationSettings { get; set; }
		ISettingsRepository SettingsRepository { get; set; }
		IPageRepository PageRepository { get; set; }

		/// <summary>
		/// Searches the lucene index with the search text.
		/// </summary>
		/// <param name="searchText">The text to search with.</param>
		/// <remarks>Syntax reference: http://lucene.apache.org/java/2_3_2/queryparsersyntax.html#Wildcard</remarks>
		/// <exception cref="SearchException">An error occurred searching the lucene.net index.</exception>
		IEnumerable<SearchResultViewModel> Search(string searchText);

		/// <summary>
		/// Adds the specified page to the search index.
		/// </summary>
		/// <param name="model">The page to add.</param>
		/// <exception cref="SearchException">An error occurred with the lucene.net IndexWriter while adding the page to the index.</exception>
		void Add(PageViewModel model);

		/// <summary>
		/// Deletes the specified page from the search indexs.
		/// </summary>
		/// <param name="model">The page to remove.</param>
		/// <exception cref="SearchException">An error occurred with the lucene.net IndexReader while deleting the page from the index.</exception>
		int Delete(PageViewModel model);

		/// <summary>
		/// Updates the <see cref="Page"/> in the search index, by removing it and re-adding it.
		/// </summary>
		/// <param name="model">The page to update</param>
		/// <exception cref="SearchException">An error occurred with lucene.net while deleting the page or inserting it back into the index.</exception>
		void Update(PageViewModel model);

		/// <summary>
		/// Creates the initial search index based on all pages in the system.
		/// </summary>
		/// <exception cref="SearchException">An error occurred with the lucene.net IndexWriter while adding the page to the index.</exception>
		void CreateIndex();
	}
}