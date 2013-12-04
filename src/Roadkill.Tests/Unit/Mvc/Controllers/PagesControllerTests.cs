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

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PagesControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private RepositoryMock _repository;
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

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_repository = _container.Repository;
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
			_pageServiceMock.Setup(x => x.GetMarkupConverter()).Returns(new MarkupConverter(_applicationSettings, _repository, _pluginFactory));
			_pageServiceMock.Setup(x => x.GetById(It.IsAny<int>(), false)).Returns<int, bool>((int id, bool loadContent) =>
				{
					Page page = _repository.GetPageById(id);
					return new PageViewModel(page);
				});
			_pageServiceMock.Setup(x => x.GetById(It.IsAny<int>(), true)).Returns<int,bool>((int id, bool loadContent) =>
			{
				PageContent content = _repository.GetLatestPageContent(id);

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
			_repository.Pages.Add(page1);
			_repository.PageContents.Add(page1Content);

			return page1;
		}

		private Page AddDummyPage2()
		{
			Page page2 = new Page() { Id = 50, Tags = "anothertag", Title = "Page 2" };
			PageContent page2Content = new PageContent() { Id = Guid.NewGuid(), Page = page2, Text = "Hello world 2" };
			_repository.Pages.Add(page2);
			_repository.PageContents.Add(page2Content);

			return page2;
		}

		[Test]
		public void AllPages_Should_Return_Model_And_Pages()
		{
			// Arrange
			Page page1 = AddDummyPage1();
			Page page2 = AddDummyPage2();
			PageContent page1Content = _repository.PageContents.First(p => p.Page.Id == page1.Id);

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
		public void AllTags_Should_Return_Model_And_Tags()
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
		public void AllTagsAsJson_Should_Return_Model_And_Tags()
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
		public void ByUser_Should_Contain_ViewData_And_Return_Model_And_Pages()
		{
			// Arrange
			string username = "amazinguser";

			Page page1 = AddDummyPage1();
			page1.CreatedBy = username;
			PageContent page1Content = _repository.PageContents.First(p => p.Page.Id == page1.Id);

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
		public void ByUser_With_Base64_Username_Should_Contain_ViewData_And_Return_Model_And_Pages()
		{
			// Arrange
			string username = @"mydomain\Das ádmin``";
			string base64Username = "bXlkb21haW5cRGFzIOFkbWluYGA=";

			Page page1 = AddDummyPage1();
			page1.CreatedBy = username;
			PageContent page1Content = _repository.PageContents.First(p => p.Page.Id == page1.Id);

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
		public void Delete_Should_Contains_Redirect_And_Remove_Page()
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
		public void Edit_GET_Should_Redirect_With_Invalid_Page_Id()
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
		public void Edit_GET_As_Editor_With_Locked_Page_Should_Return_403()
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
		public void Edit_GET_Should_Return_ViewResult()
		{
			// Arrange
			Page page = AddDummyPage1();
			PageContent pageContent = _repository.PageContents.First(p => p.Page.Id == page.Id);
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
		public void Edit_POST_Should_Return_RedirectResult_And_Call_PageService()
		{
			// Arrange
			_contextStub.CurrentUser = "Admin";
			Page page = AddDummyPage1();
			PageContent pageContent = _repository.PageContents[0];

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
			Assert.That(_repository.Pages.Count, Is.EqualTo(1));
			_pageServiceMock.Verify(x => x.UpdatePage(model));
		}

		[Test]
		public void Edit_POST_With_Invalid_Data_Should_Return_View_And_Invalid_ModelState()
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
		public void GetPreview_Should_Return_JavascriptResult_And_Page_Content()
		{
			// Arrange
			_contextStub.CurrentUser = "Admin";
			Page page = AddDummyPage1();

			// Act
			ActionResult result = _pagesController.GetPreview(_repository.PageContents[0].Text);

			// Assert
			_pageServiceMock.Verify(x => x.GetMarkupConverter());

			Assert.That(result, Is.TypeOf<JavaScriptResult>(), "JavaScriptResult");
			JavaScriptResult javascriptResult = result as JavaScriptResult;
			Assert.That(javascriptResult.Script, Contains.Substring(_repository.PageContents[0].Text));
		}

		[Test]
		public void History_Returns_ViewResult_And_Model_With_Two_Versions()
		{
			// Arrange
			Page page = AddDummyPage1();
			_repository.PageContents.Add(new PageContent() { VersionNumber = 2, Page = page, Id = Guid.NewGuid(), Text = "v2text" });

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
		public void New_GET_Should_Return_ViewResult()
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
		public void New_POST_Should_Return_RedirectResult_And_Call_PageService()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.RawTags = "newtag1,newtag2";
			model.Title = "New page title";
			model.Content = "*Some new content here*";

			_pageServiceMock.Setup(x => x.AddPage(model)).Returns(() =>
			{
				_repository.Pages.Add(new Page() { Id = 50, Title = model.Title, Tags = model.RawTags });
				_repository.PageContents.Add(new PageContent() { Id = Guid.NewGuid(), Text = model.Content });
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
			Assert.That(_repository.Pages.Count, Is.EqualTo(1));
			_pageServiceMock.Verify(x => x.AddPage(model));
		}

		[Test]
		public void New_POST_With_Invalid_Data_Should_Return_View_And_Invalid_ModelState()
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
		public void Revert_Should_Return_RedirectToRouteResult_With_Page_Id()
		{
			// Arrange
			_contextStub.IsAdmin = true;
			Page page = AddDummyPage1();

			Guid version2Guid = Guid.NewGuid();
			Guid version3Guid = Guid.NewGuid();

			_repository.PageContents.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text" });
			_repository.PageContents.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text" });

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
		public void Revert_As_Editor_And_Locked_Page_Should_Return_RedirectToRouteResult_To_Index()
		{
			// Arrange
			Page page = AddDummyPage1();
			page.IsLocked = true;

			Guid version2Guid = Guid.NewGuid();
			Guid version3Guid = Guid.NewGuid();

			_repository.PageContents.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text" });
			_repository.PageContents.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text" });

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
		public void Tag_Returns_ViewResult_And_Calls_PageService()
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
		public void Version_Should_Return_ViewResult_And_PageSummary_Model()
		{
			// Arrange
			Page page = AddDummyPage1();
			page.IsLocked = true;

			Guid version2Guid = Guid.NewGuid();
			Guid version3Guid = Guid.NewGuid();

			_repository.PageContents.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text" });
			_repository.PageContents.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text" });

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
