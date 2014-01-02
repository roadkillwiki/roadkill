using System;
using System.Collections.Generic;
using System.IO;
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
using System.Runtime.Caching;
using Roadkill.Tests.Unit.StubsAndMocks;
using System.Web;
using MvcContrib.TestHelper;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class WikiControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;

		private WikiController _wikiController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;	
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;

			_wikiController = new WikiController(_applicationSettings, _userService, _pageService, _context, _settingsService);
			_wikiController.SetFakeControllerContext();
		}

		[Test]
		public void Index_Should_Return_Page()
		{
			// Arrange
			Page page1 = new Page()
			{
				Id = 50,
				Tags = "homepage, tag2",
				Title = "Welcome to the site"
			};
			PageContent page1Content = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page1,
				Text = "Hello world"
			};
			_repository.Pages.Add(page1);
			_repository.PageContents.Add(page1Content);

			// Act
			ActionResult result = _wikiController.Index(50, "");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			PageViewModel model = result.ModelFromActionResult<PageViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.Title, Is.EqualTo(page1.Title));
			Assert.That(model.Content, Is.EqualTo(page1Content.Text));
		}

		[Test]
		public void Index_With_Bad_Page_Id_Should_Redirect()
		{
			// Arrange

			// Act
			ActionResult result = _wikiController.Index(0, "");

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "ViewResult");

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void Index_With_Unknown_Page_Should_Throw_404Exception()
		{
			// Arrange

			// Act + Assert
			try
			{
				_wikiController.Index(5, "");
				Assert.Fail("No Exception was thrown");
			}
			catch (HttpException ex)
			{
				if (ex.GetHttpCode() != 404)
					Assert.Fail("HttpException was thrown, but the status code was "+ ex.GetHttpCode()+ " and not a 404.");
			}
			catch (Exception ex)
			{
				Assert.Fail("Expected HttpException but was " + ex.GetType().Name);
			}

		}

		[Test]
		public void ServerError_Should_Return_500_View()
		{
			// Arrange

			// Act
			ActionResult result = _wikiController.ServerError();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("500"));
		}

		[Test]
		public void NotFound_Should_Return_500_View()
		{
			// Arrange

			// Act
			ActionResult result = _wikiController.NotFound();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("404"));
		}

		[Test]
		public void PageToolbar_Should_Return_PartialView()
		{
			// Arrange
			_repository.AddNewPage(new Page() {Title = "Title" }, "text", "admin", DateTime.UtcNow);

			// Act
			ActionResult result = _wikiController.PageToolbar(1);

			// Assert
			PartialViewResult partialResult = result.AssertResultIs<PartialViewResult>();
			partialResult.AssertPartialViewRendered();
		}

		[Test]
		public void PageToolbar_Should_Return_Empty_Content_When_Page_Cannot_Be_Found()
		{
			// Arrange

			// Act
			ActionResult result = _wikiController.PageToolbar(666);

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.Not.Null);
		}

		[Test]
		public void PageToolbar_Should_Return_Empty_Content_When_Id_Is_Null()
		{
			// Arrange

			// Act
			ActionResult result = _wikiController.PageToolbar(null);

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.Not.Null);
		}
	}
}
