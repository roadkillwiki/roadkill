using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.WebApi
{
	/// <summary>
	/// REST api for searching: GET api/search?query=xyz, GET api/search/createindex
	/// </summary>
	[ApiController]
	[Route("api/search")]
	[ApiKeyAuthorize]
	public class SearchController : Microsoft.AspNetCore.Mvc.ControllerBase
	{
		private readonly SearchService _searchService;

		public SearchController(SearchService searchService)
		{
			_searchService = searchService;
		}

		[HttpGet]
		public IEnumerable<SearchResultViewModel> Get([FromQuery] string query)
		{
			return _searchService.Search(query);
		}

		[HttpGet("CreateIndex")]
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
