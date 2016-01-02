using System.Collections.Generic;
using System.Web.Http;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.WebApi
{
	[RoutePrefix("api/search")]
	[ApiKeyAuthorize]
	public class SearchController : ApiController
	{
		private readonly SearchService _searchService;

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchController"/> class.
		/// </summary>
		/// <param name="searchService">The search service.</param>
		public SearchController(SearchService searchService)
		{
			_searchService = searchService;
		}

		/// <summary>
		/// Searches the roadkill instance with the text provided.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<SearchResultViewModel> Get(string query)
		{
			return _searchService.Search(query);
		}

		/// <summary>
		/// Creates or re-indexes and updates the Lucene index with all roadkill pages.
		/// </summary>
		/// <returns>"OK" if there no errors occurred, otherwise any error message.</returns>
		[HttpGet]
		[Route("CreateIndex")]
		public string CreateIndex()
		{
			try
			{
				_searchService.CreateIndex();
				return "OK";
			}
			catch (SearchException ex)
			{
				return ex.ToString();
			}
		}
	}
}
