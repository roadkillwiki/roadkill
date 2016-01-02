using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebApi;
using Roadkill.Core.Services;

namespace Roadkill.Tests.Unit.Mvc.WebApi
{
	[TestFixture]
	[Category("Unit")]
	public class SearchControllerTests
	{
		private MocksAndStubsContainer _container;

		private PageService _pageService;
		private SearchService _searchService;
		private SearchController _searchController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_pageService = _container.PageService;
			_searchService = _container.SearchService;

			_searchController = new SearchController(_searchService);
		}

		[Test]
		public void get_should_search_return_correct_results_using_search_service()
		{
			// Arrange
			_pageService.AddPage(new PageViewModel() { Id = 1, Title = "title 1", Content = "page 1" });
			_pageService.AddPage(new PageViewModel() { Id = 2, Title = "title 2", Content = "page 2" });
			_pageService.AddPage(new PageViewModel() { Id = 3, Title = "new page 3", Content = "page 3" });

			// Act
			IEnumerable<SearchResultViewModel> searchResults = _searchController.Get("title");

			// Assert
			Assert.That(searchResults.Count(), Is.EqualTo(2));
		}
	}
}