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

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class HelpControllerTests
	{
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceBase _userService;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;

		private HelpController _controller;

		[SetUp]
		public void Setup()
		{
			_context = new Mock<IUserContext>().Object;
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.Installed = true;
			_repository = new RepositoryMock();

			// Cache
			ListCache listCache = new ListCache(_applicationSettings, CacheMock.RoadkillCache);
			PageViewModelCache pageViewModelCache = new PageViewModelCache(_applicationSettings, CacheMock.RoadkillCache);
			SiteCache siteCache = new SiteCache(_applicationSettings, CacheMock.RoadkillCache);

			// Dependencies for PageService
			_pluginFactory = new PluginFactoryMock();
			Mock<SearchService> searchMock = new Mock<SearchService>();

			_settingsService = new SettingsService(_applicationSettings, _repository);
			_userService = new Mock<UserServiceBase>(_applicationSettings, null).Object;
			_historyService = new PageHistoryService(_applicationSettings, _repository, _context, pageViewModelCache, _pluginFactory);
			_pageService = new PageService(_applicationSettings, _repository, null, _historyService, _context, listCache, pageViewModelCache, siteCache, _pluginFactory);

			_controller = new HelpController(_applicationSettings, _userService, _context, _settingsService, _pageService);
		}

		[Test]
		public void Index_Should_Return_ViewResult()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Mediawiki";

			// Act
			ViewResult result = _controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void About_Should_Return_ViewResult_And_Page_With_About_Tag_As_Model()
		{
			// Arrange
			Page aboutPage = new Page()
			{
				Id = 1,
				Title = "about",
				Tags = "about"
			};

			_repository.AddNewPage(aboutPage, "text", "nobody", DateTime.Now);

			// Act
			ViewResult result = _controller.About() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			PageViewModel model = result.ModelFromActionResult<PageViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.Id, Is.EqualTo(aboutPage.Id));
			Assert.That(model.Title, Is.EqualTo(aboutPage.Title));
		}

		[Test]
		public void About_Should_Return_RedirectResult_To_New_Page_When_No_Page_Has_About_Tag()
		{
			// Arrange


			// Act
			RedirectToRouteResult result = _controller.About() as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			Assert.That(result.RouteValues["controller"], Is.EqualTo("Pages"));
			Assert.That(result.RouteValues["action"], Is.EqualTo("New"));
			Assert.That(result.RouteValues["title"], Is.EqualTo("about"));
			Assert.That(result.RouteValues["tags"], Is.EqualTo("about"));
		}

		[Test]
		public void CreoleReference_Should_Return_View()
		{
			// Arrange


			// Act
			ViewResult result = _controller.CreoleReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void MediaWikiReference_Should_Return_View()
		{
			// Arrange


			// Act
			ViewResult result = _controller.MediaWikiReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void MarkdownReference_Should_Return_View()
		{
			// Arrange


			// Act
			ViewResult result = _controller.MarkdownReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}
	}
}
