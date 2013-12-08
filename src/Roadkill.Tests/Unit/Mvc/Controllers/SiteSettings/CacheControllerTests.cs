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

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class CacheControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private SettingsService _settingsService;
		private PageViewModelCache _pageCache;
		private ListCache _listCache;
		private SiteCache _siteCache;

		private CacheController _cacheController;

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

			_cacheController = new CacheController(_applicationSettings, _userService, _settingsService, _context, _listCache, _pageCache, _siteCache);
		}

		[Test]
		public void Index_Should_Return_ViewModel_With_Filled_Properties()
		{
			// Arrange
			_applicationSettings.UseObjectCache = true;			
			_pageCache.Add(1, new PageViewModel());
			_listCache.Add<string>("test", new List<string>());
			_siteCache.AddMenu("menu");

			// Act
			ActionResult result = _cacheController.Index();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			CacheViewModel model = result.ModelFromActionResult<CacheViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.IsCacheEnabled, Is.True);
			Assert.That(model.PageKeys.Count(), Is.EqualTo(1));
			Assert.That(model.ListKeys.Count(), Is.EqualTo(1));
			Assert.That(model.SiteKeys.Count(), Is.EqualTo(1));
		}
	}
}
