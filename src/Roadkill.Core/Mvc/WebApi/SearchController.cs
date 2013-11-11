using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers.Api
{
	[WebApiAdminRequired]
	public class SearchController : ApiControllerBase
	{
		private SearchService _searchService;

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
	}
}
