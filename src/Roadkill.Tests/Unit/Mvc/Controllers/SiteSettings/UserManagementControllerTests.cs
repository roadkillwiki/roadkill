using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Localization;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using Roadkill.Core.Import;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class UserManagementControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private IWikiImporter _wikiImporter;
		private PluginFactoryMock _pluginFactory;
		private SearchService _searchService;
		private SettingsService _settingsService;
		private PageViewModelCache _pageCache;
		private ListCache _listCache;
		private SiteCache _siteCache;
		private MemoryCache _cache;

		private UserManagementController _controller;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_pageCache = _container.PageViewModelCache;
			_listCache = _container.ListCache;
			_siteCache = _container.SiteCache;
			_cache = _container.MemoryCache;

			_pageService = _container.PageService;
			_wikiImporter = new ScrewTurnImporter(_applicationSettings, _repository);
			_pluginFactory = _container.PluginFactory;
			_searchService = _container.SearchService;

			_controller = new UserManagementController(_applicationSettings, _userService, _settingsService, _pageService, 
				_searchService, _context, _listCache, _pageCache, _siteCache, _wikiImporter, _repository, _pluginFactory);
		}

		[Test]
		public void AddAdmin_GET_Should()
		{
			// Arrange

			// Act
			ActionResult result = _controller.AddAdmin();

			// Assert
		}

		[Test]
		public void AddAdmin_POST_Should()
		{
			// Arrange

			// Act
			ActionResult result = _controller.AddAdmin(null);

			// Assert
		}

		[Test]
		public void AddEditor_GET_Should()
		{
			// Arrange

			// Act
			ActionResult result = _controller.AddEditor();

			// Assert
		}

		[Test]
		public void AddEditor_POST_Should()
		{
			// Arrange

			// Act
			ActionResult result = _controller.AddEditor(null);

			// Assert
		}

		[Test]
		public void DeleteUser_Should()
		{
			// Arrange

			// Act
			ActionResult result = _controller.DeleteUser("");

			// Assert
		}

		[Test]
		public void EditUser_Get_Should()
		{
			// Arrange

			// Act
			ActionResult result = _controller.EditUser(Guid.Empty);

			// Assert
		}

		[Test]
		public void EditUser_Post_Should()
		{
			// Arrange

			// Act
			ActionResult result = _controller.EditUser(new UserViewModel());

			// Assert
		}

		[Test]
		public void Index_Should()
		{
			// Arrange

			// Act
			ActionResult result = _controller.Index();

			// Assert
		}
	}
}
