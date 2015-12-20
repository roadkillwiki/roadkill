using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
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
	public class PagesControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private PageRepositoryMock _pageRepository;
		private UserServiceMock _userService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;
		private MarkupConverter _markupConverter;
		private SearchServiceMock _searchService;

		private UserContextStub _contextStub;
		private Mock<IPageService> _pageServiceMock;
		private IPageService _pageService;
		private MvcMockContainer _mocksContainer;
		private PagesController _pagesController;
		private SettingsRepositoryMock _settingsRepository;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;

			_settingsRepository = _container.SettingsRepository;
			_pageRepository = _container.PageRepository;
			
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_markupConverter = _container.MarkupConverter;
			_searchService = _container.SearchService;

			// Use a stub instead of the MocksAndStubsContainer's default
			_contextStub = new UserContextStub();

			// Customise the page service so we can verify what was called
			_pageServiceMock = new Mock<IPageService>();
			_pageServiceMock.Setup(x => x.GetMarkupConverter()).Returns(new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory));
			_pageServiceMock.Setup(x => x.GetById(It.IsAny<int>(), false)).Returns<int, bool>((int id, bool loadContent) =>
				{
					Page page = _pageRepository.GetPageById(id);
					return new PageViewModel(page);
				});
			_pageServiceMock.Setup(x => x.GetById(It.IsAny<int>(), true)).Returns<int,bool>((int id, bool loadContent) =>
			{
				PageContent content = _pageRepository.GetLatestPageContent(id);

				if (content != null)
					return new PageViewModel(content, _markupConverter);
				else
					return null;
			});
			_pageServiceMock.Setup(x => x.FindByTag(It.IsAny<string>()));
			_pageService = _pageServiceMock.Object;

			_pagesController = new PagesController(_applicationSettings, _userService, _settingsService, _pageService, _searchService, _historyService, _contextStub);
			_mocksContainer = _pagesController.SetFakeControllerContext();
		}

		private Page AddDummyPage1()
		{
			Page page1 = new Page() { Id = 1, Tags = "tag1,tag2", Title = "Welcome to the site", CreatedBy = "admin" };
			PageContent page1Content = new PageContent() { Id = Guid.NewGuid(), Page = page1, Text = "Hello world 1", VersionNumber = 1 };
			_pageRepository.Pages.Add(page1);
			_pageRepository.PageContents.Add(page1Content);

			return page1;
		}

		private Page AddDummyPage2()
		{
			Page page2 = new Page() { Id = 50, Tags = "anothertag", Title = "Page 2" };
			PageContent page2Content = new PageContent() { Id = Guid.NewGuid(), Page = page2, Text = "Hello world 2" };
			_pageRepository.Pages.Add(page2);
			_pageRepository.PageContents.Add(page2Content);

			return page2;
		}

		[Test]
		public void allpages_should_return_model_and_pages()
		{
			// Arrange
			Page page1 = AddDummyPage1();
			Page page2 = AddDummyPage2();
			PageContent page1Content = _pageRepository.PageContents.First(p => p.Page.Id == page1.Id);

			// Act
			ActionResult result = _pagesController.AllPages();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			IEnumerable<PageViewModel> model = result.ModelFromActionResult<IEnumerable<PageViewModel>>();
			Assert.NotNull(model, "Null model");

			List<PageViewModel> summaryList = model.OrderBy(p => p.Id).ToList();
			_pageServiceMock.Verify(x => x.AllPages(false));
		}

		[Test]
		public void alltags_should_return_model_and_tags()
		{
			// Arrange
			Page page1 = AddDummyPage1();
			page1.Tags = "a-tag,b-tag";

			Page page2 = AddDummyPage2();
			page2.Tags = "z-tag,a-tag";

			// Act
			ActionResult result = _pagesController.AllTags();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			IEnumerable<TagViewModel> model = result.ModelFromActionResult<IEnumerable<TagViewModel>>();
			Assert.NotNull(model, "Null model");
			_pageServiceMock.Verify(x => x.AllTags());
		}

		[Test]
		public void alltagsasjson_should_return_model_and_tags()
		{
			// Arrange
			Page page1 = AddDummyPage1();
			page1.Tags = "a-tag,b-tag";

			Page page2 = AddDummyPage2();
			page2.Tags = "z-tag,a-tag";

			// Act
			ActionResult result = _pagesController.AllTagsAsJson();		

			// Assert
			Assert.That(result, Is.TypeOf<JsonResult>(), "JsonResult");

			JsonResult jsonResult = result as JsonResult;
			Assert.NotNull(jsonResult, "Null jsonResult");

			var xxx = jsonResult.Data.GetType();
			Assert.That(jsonResult.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.AllowGet));
			Assert.That(jsonResult.Data, Is.AssignableTo<IEnumerable<string>>());
			_pageServiceMock.Verify(x => x.AllTags());
		}

		[Test]
		public void byuser_should_contain_viewdata_and_return_model_and_pages()
		{
			// Arrange
			string username = "amazinguser";

			Page page1 = AddDummyPage1();
			page1.CreatedBy = username;
			PageContent page1Content = _pageRepository.PageContents.First(p => p.Page.Id == page1.Id);

			Page page2 = AddDummyPage2();
			page2.CreatedBy = username;

			// Act
			ActionResult result = _pagesController.ByUser(username, false);

			// Assert
			Assert.That(_pagesController.ViewData.Keys.Count, Is.GreaterThanOrEqualTo(1));

			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			IEnumerable<PageViewModel> model = result.ModelFromActionResult<IEnumerable<PageViewModel>>();
			Assert.NotNull(model, "Null model");
			_pageServiceMock.Verify(x => x.AllPagesCreatedBy(username));
		}

		[Test]
		public void byuser_with_base64_username_should_contain_viewdata_and_return_model_and_pages()
		{
			// Arrange
			string username = @"mydomain\Das ádmin``";
			string base64Username = "bXlkb21haW5cRGFzIOFkbWluYGA=";

			Page page1 = AddDummyPage1();
			page1.CreatedBy = username;
			PageContent page1Content = _pageRepository.PageContents.First(p => p.Page.Id == page1.Id);

			Page page2 = AddDummyPage2();
			page2.CreatedBy = username;

			// Act
			ActionResult result = _pagesController.ByUser(base64Username, true);

			// Assert
			Assert.That(_pagesController.ViewData.Keys.Count, Is.GreaterThanOrEqualTo(1));

			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			IEnumerable<PageViewModel> model = result.ModelFromActionResult<IEnumerable<PageViewModel>>();
			Assert.NotNull(model, "Null model");
			_pageServiceMock.Verify(x => x.AllPagesCreatedBy(username));
		}

		[Test]
		public void delete_should_contains_redirect_and_remove_page()
		{
			// Arrange
			Page page1 = AddDummyPage1();
			Page page2 = AddDummyPage2();

			// Act
			ActionResult result = _pagesController.Delete(50);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult");
			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.NotNull(redirectResult, "Null RedirectToRouteResult");

			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("AllPages"));
			_pageServiceMock.Verify(x => x.DeletePage(50));
		}

		[Test]
		public void edit_get_should_redirect_with_invalid_page_id()
		{
			// Arrange

			// Act
			ActionResult result = _pagesController.Edit(1);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult");
			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.NotNull(redirectResult, "Null RedirectToRouteResult");

			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("New"));
		}

		[Test]
		public void edit_get_as_editor_with_locked_page_should_return_403()
		{
			// Arrange
			_contextStub.IsAdmin = false;
			Page page = AddDummyPage1();
			page.IsLocked = true;
			_pageServiceMock.Setup(x => x.GetById(page.Id, It.IsAny<bool>())).Returns(new PageViewModel() { Id = page.Id, IsLocked = true });

			// Act
			ActionResult result = _pagesController.Edit(page.Id);

			// Assert
			Assert.That(result, Is.TypeOf<HttpStatusCodeResult>(), "HttpStatusCodeResult");
			HttpStatusCodeResult statusResult = result as HttpStatusCodeResult;
			Assert.NotNull(statusResult, "Null HttpStatusCodeResult");

			Assert.That(statusResult.StatusCode, Is.EqualTo(403));
		}

		[Test]
		public void edit_get_should_return_viewresult()
		{
			// Arrange
			Page page = AddDummyPage1();
			PageContent pageContent = _pageRepository.PageContents.First(p => p.Page.Id == page.Id);
			_pageServiceMock.Setup(x => x.GetById(page.Id, It.IsAny<bool>())).Returns(new PageViewModel() { Id = page.Id, });

			// Act
			ActionResult result = _pagesController.Edit(page.Id);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			ViewResult viewResult = result as ViewResult;
			Assert.NotNull(viewResult, "Null viewResult");
			_pageServiceMock.Verify(x => x.GetById(page.Id, It.IsAny<bool>()));
		}

		[Test]
		public void edit_post_should_return_redirectresult_and_call_pageservice()
		{
			// Arrange
			_contextStub.CurrentUser = "Admin";
			Page page = AddDummyPage1();
			PageContent pageContent = _pageRepository.PageContents[0];

			PageViewModel model = new PageViewModel();
			model.Id = page.Id;
			model.RawTags = "newtag1,newtag2";
			model.Title = "New page title";
			model.Content = "*Some new content here*";

			// Act
			ActionResult result = _pagesController.Edit(model);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult");
			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.NotNull(redirectResult, "Null RedirectToRouteResult");

			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Wiki"));
			Assert.That(_pageRepository.Pages.Count, Is.EqualTo(1));
			_pageServiceMock.Verify(x => x.UpdatePage(model));
		}

		[Test]
		public void edit_post_with_invalid_data_should_return_view_and_invalid_modelstate()
		{
			// Arrange
			_contextStub.CurrentUser = "Admin";
			Page page = AddDummyPage1();

			PageViewModel model = new PageViewModel();
			model.Id = page.Id;

			// Act
			_pagesController.ModelState.AddModelError("Title", "You forgot it");
			ActionResult result = _pagesController.Edit(model);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			Assert.False(_pagesController.ModelState.IsValid);
		}

		[Test]
		public void getpreview_should_return_javascriptresult_and_page_content()
		{
			// Arrange
			_contextStub.CurrentUser = "Admin";
			Page page = AddDummyPage1();

			// Act
			ActionResult result = _pagesController.GetPreview(_pageRepository.PageContents[0].Text);

			// Assert
			_pageServiceMock.Verify(x => x.GetMarkupConverter());

			Assert.That(result, Is.TypeOf<JavaScriptResult>(), "JavaScriptResult");
			JavaScriptResult javascriptResult = result as JavaScriptResult;
			Assert.That(javascriptResult.Script, Contains.Substring(_pageRepository.PageContents[0].Text));
		}

		[Test]
		public void history_returns_viewresult_and_model_with_two_versions()
		{
			// Arrange
			Page page = AddDummyPage1();
			_pageRepository.PageContents.Add(new PageContent() { VersionNumber = 2, Page = page, Id = Guid.NewGuid(), Text = "v2text" });

			// Act
			ActionResult result = _pagesController.History(page.Id);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			ViewResult viewResult = result as ViewResult;

			List<PageHistoryViewModel> model = viewResult.ModelFromActionResult<IEnumerable<PageHistoryViewModel>>().ToList();
			Assert.That(model.Count, Is.EqualTo(2));
			Assert.That(model[0].PageId, Is.EqualTo(page.Id));
			Assert.That(model[1].PageId, Is.EqualTo(page.Id));
			Assert.That(model[0].VersionNumber, Is.EqualTo(2)); // latest first
			Assert.That(model[1].VersionNumber, Is.EqualTo(1));
		}

		[Test]
		public void new_get_should_return_viewresult()
		{
			// Arrange
			string title = "my title";

			// Act
			ActionResult result = _pagesController.New(title);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ActionResult is not a ViewResult");
			ViewResult viewResult = result as ViewResult;
			Assert.NotNull(viewResult, "Null viewResult");
			Assert.That(viewResult.ViewName, Is.EqualTo("Edit"));
			
			PageViewModel model = viewResult.ModelFromActionResult<PageViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.Title, Is.EqualTo(title));
		}

		[Test]
		public void new_post_should_return_redirectresult_and_call_pageservice()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "newtag1,newtag2";
			model.Title = "New page title";
			model.Content = "*Some new content here*";

			_pageServiceMock.Setup(x => x.AddPage(model)).Returns(() =>
			{
				_pageRepository.Pages.Add(new Page() { Id = 50, Title = model.Title, Tags = model.RawTags });
				_pageRepository.PageContents.Add(new PageContent() { Id = Guid.NewGuid(), Text = model.Content });
				model.Id = 50;

				return model;
			});

			// Act
			ActionResult result = _pagesController.New(model);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult not returned");
			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.NotNull(redirectResult, "Null RedirectToRouteResult");

			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Wiki"));
			Assert.That(_pageRepository.Pages.Count, Is.EqualTo(1));
			_pageServiceMock.Verify(x => x.AddPage(model));
		}

		[Test]
		public void new_post_with_invalid_data_should_return_view_and_invalid_modelstate()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.Title = "";

			// Act
			_pagesController.ModelState.AddModelError("Title", "You forgot it");
			ActionResult result = _pagesController.New(model);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			Assert.False(_pagesController.ModelState.IsValid);
		}

		[Test]
		public void revert_should_return_redirecttorouteresult_with_page_id()
		{
			// Arrange
			_contextStub.IsAdmin = true;
			Page page = AddDummyPage1();

			Guid version2Guid = Guid.NewGuid();
			Guid version3Guid = Guid.NewGuid();

			_pageRepository.PageContents.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text" });
			_pageRepository.PageContents.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text" });

			// Act
			ActionResult result = _pagesController.Revert(version2Guid, page.Id);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult not returned");
			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.NotNull(redirectResult, "Null RedirectToRouteResult");

			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("History"));
			Assert.That(redirectResult.RouteValues["id"], Is.EqualTo(1));
		}

		[Test]
		public void revert_as_editor_and_locked_page_should_return_redirecttorouteresult_to_index()
		{
			// Arrange
			Page page = AddDummyPage1();
			page.IsLocked = true;

			Guid version2Guid = Guid.NewGuid();
			Guid version3Guid = Guid.NewGuid();

			_pageRepository.PageContents.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text" });
			_pageRepository.PageContents.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text" });

			// Act
			ActionResult result = _pagesController.Revert(version2Guid, page.Id);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult not returned");
			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.NotNull(redirectResult, "Null RedirectToRouteResult");

			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}

		[Test]
		public void tag_returns_viewresult_and_calls_pageservice()
		{
			// Arrange
			Page page1 = AddDummyPage1();
			Page page2 = AddDummyPage2();
			page1.Tags = "tag1,tag2";
			page2.Tags = "tag2";

			// Act
			ActionResult result = _pagesController.Tag("tag2");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			ViewResult viewResult = result as ViewResult;
			_pageServiceMock.Verify(x => x.FindByTag("tag2"));
		}

		[Test]
		public void version_should_return_viewresult_and_pagesummary_model()
		{
			// Arrange
			Page page = AddDummyPage1();
			page.IsLocked = true;

			Guid version2Guid = Guid.NewGuid();
			Guid version3Guid = Guid.NewGuid();

			_pageRepository.PageContents.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text" });
			_pageRepository.PageContents.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text" });

			// Act
			ActionResult result = _pagesController.Version(version2Guid);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			ViewResult viewResult = result as ViewResult;
			Assert.NotNull(viewResult, "Null ViewResult");

			PageViewModel model = viewResult.ModelFromActionResult<PageViewModel>();
			Assert.That(model.Content, Contains.Substring("version2 text"));
		}
	}
}
