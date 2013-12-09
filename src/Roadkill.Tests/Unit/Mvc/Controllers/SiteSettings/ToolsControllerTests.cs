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
	public class ToolsControllerTests
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

		private ToolsController _toolsController;

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

			_toolsController = new ToolsController(_applicationSettings, _userService, _settingsService, _pageService, 
				_searchService, _context, _listCache, _pageCache, _siteCache, _wikiImporter, _repository, _pluginFactory);
		}

		[Test]
		public void ClearPages_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.ClearPages();

			// Assert
		}

		[Test]
		public void ExportAsSql_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.ExportAsSql();

			// Assert
		}

		[Test]
		public void ExportAsWikiFiles_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.ExportAsWikiFiles();

			// Assert
		}

		[Test]
		public void ExportAsXml_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.ExportAsXml();

			// Assert
		}

		[Test]
		public void ExportAttachments_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.ExportAttachments();

			// Assert
		}

		[Test]
		public void ImportFromScrewTurn_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.ImportFromScrewTurn("");

			// Assert
		}

		[Test]
		public void Index_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.Index();

			// Assert
		}

		[Test]
		public void RenameTag_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.RenameTag("", "");

			// Assert
		}

		[Test]
		public void UpdateSearchIndex_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.UpdateSearchIndex();

			// Assert
		}
	}
}
