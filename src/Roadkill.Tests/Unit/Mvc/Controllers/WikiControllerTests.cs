using System;
using System.Web;
using System.Web.Mvc;
using MvcContrib.TestHelper;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;
using Roadkill.Tests.Unit.StubsAndMocks.Mvc;

namespace Roadkill.Tests.Unit.Mvc.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class WikiControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private PageRepositoryMock _pageRepository;
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
			_pageRepository = _container.PageRepository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;

			_wikiController = new WikiController(_applicationSettings, _userService, _pageService, _context, _settingsService);
			_wikiController.SetFakeControllerContext();
		}

		[Test]
		public void index_should_return_page()
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
			_pageRepository.Pages.Add(page1);
			_pageRepository.PageContents.Add(page1Content);

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
		public void index_with_bad_page_id_should_redirect()
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
		public void index_with_unknown_page_should_throw_404exception()
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
		public void servererror_should_return_500_view()
		{
			// Arrange

			// Act
			ActionResult result = _wikiController.ServerError();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("500"));
		}

		[Test]
		public void notfound_should_return_500_view()
		{
			// Arrange

			// Act
			ActionResult result = _wikiController.NotFound();

			// Assert
			ViewResult viewResult = result.AssertResultIs<ViewResult>();
			Assert.That(viewResult.ViewName, Is.EqualTo("404"));
		}

		[Test]
		public void pagetoolbar_should_return_partialview()
		{
			// Arrange
			_pageRepository.AddNewPage(new Page() {Title = "Title" }, "text", "admin", DateTime.UtcNow);

			// Act
			ActionResult result = _wikiController.PageToolbar(1);

			// Assert
			PartialViewResult partialResult = result.AssertResultIs<PartialViewResult>();
			partialResult.AssertPartialViewRendered();
		}

		[Test]
		public void pagetoolbar_should_return_empty_content_when_page_cannot_be_found()
		{
			// Arrange

			// Act
			ActionResult result = _wikiController.PageToolbar(666);

			// Assert
			ContentResult contentResult = result.AssertResultIs<ContentResult>();
			Assert.That(contentResult.Content, Is.Not.Null);
		}

		[Test]
		public void pagetoolbar_should_return_empty_content_when_id_is_null()
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
