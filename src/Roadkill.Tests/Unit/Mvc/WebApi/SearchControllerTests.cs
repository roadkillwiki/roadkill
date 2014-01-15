using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Tests.Unit.WebApi
{
	[TestFixture]
	[Category("Unit")]
	public class SearchControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private UserServiceMock _userService;
		private IUserContext _userContext;
		private RepositoryMock _repositoryMock;
		private PageService _pageService;
		private SearchService _searchService;
		private SearchController _searchController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_userContext = _container.UserContext;
			_userService = _container.UserService;
			_repositoryMock = _container.Repository;
			_pageService = _container.PageService;
			_searchService = _container.SearchService;

			_searchController = new SearchController(_searchService, _applicationSettings, _userService, _userContext);
		}

		[Test]
		public void Get_Should_Search_Return_Correct_Results_Using_Search_Service()
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