using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers.Api
{
	[WebApiAdminRequired]
	[RoutePrefix("api/search")]
	public class SearchController : ApiControllerBase
	{
		private readonly SearchService _searchService;

		/// <summary>
		/// Initializes a new instance of the <see cref="SearchController"/> class.
		/// </summary>
		/// <param name="searchService">The search service.</param>
		public SearchController(SearchService searchService, ApplicationSettings appSettings, UserServiceBase userService, IUserContext userContext)
			: base(appSettings, userService, userContext)
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
