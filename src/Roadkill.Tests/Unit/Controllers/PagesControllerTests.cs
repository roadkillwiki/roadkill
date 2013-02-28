
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Search;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PagesControllerTests
	{
		private IConfigurationContainer _config;
		private IRepository _repository;
		private Mock<IRepository> _mockRepository;

		private UserManager _userManager;
		private IPageManager _pageManager;
		private Mock<IPageManager> _pageManagerMock;

		private HistoryManager _historyManager;
		private SettingsManager _settingsManager;
		private SearchManager _searchManager;
		private PagesController _pagesController;
		private MvcMockContainer _mocksContainer;
		private RoadkillContextStub _contextStub;
		private MarkupConverter _markupConverter;
		
		private List<Page> _pages;
		private List<PageContent> _pagesContent;

		[SetUp]
		public void Setup()
		{
			_pages = new List<Page>();
			_pagesContent = new List<PageContent>();
			_contextStub = new RoadkillContextStub();

			_config = new RoadkillSettings();
			_config.ApplicationSettings = new ApplicationSettings();
			_config.SitePreferences = new SitePreferences() { AllowedFileTypes = "png, jpg" };
			_config.ApplicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");

			// Dependencies for PageManager
			_mockRepository = new Mock<IRepository>();
			_mockRepository.Setup(x => x.GetPageById(It.IsAny<int>())).Returns<int>(x => _pages.FirstOrDefault(p => p.Id == x));
			_mockRepository.Setup(x => x.GetPageContentByVersionId(It.IsAny<Guid>())).Returns<Guid>(x => _pagesContent.FirstOrDefault(p => p.Id == x));
			_mockRepository.Setup(x => x.GetLatestPageContent(It.IsAny<int>())).Returns<int>((id) => _pagesContent.FirstOrDefault(p => p.Page.Id == id));
			_mockRepository.Setup(x => x.Delete<Page>(It.IsAny<Page>())).Callback<Page>(page => _pages.Remove(_pages.First(p => p.Id == page.Id)));
			_mockRepository.Setup(x => x.FindPagesContainingTag(It.IsAny<string>())).Returns<string>(x => _pages.Where(p => p.Tags.ToLower().Contains(x.ToLower())));
			_mockRepository.Setup(x => x.FindPageContentsByPageId(It.IsAny<int>())).Returns<int>(x => _pagesContent.Where(p => p.Page.Id == x));

			_repository = _mockRepository.Object;
			_userManager = new Mock<UserManager>(_config, _repository).Object;
			_historyManager = new HistoryManager(_config, _repository, _contextStub);
			_settingsManager = new SettingsManager(_config, _repository);
			_searchManager = new SearchManager(_config, _repository);

			_markupConverter = new MarkupConverter(_config, _repository);
			_pageManagerMock = new Mock<IPageManager>();
			_pageManagerMock.Setup(x => x.GetMarkupConverter()).Returns(new MarkupConverter(_config, _repository));
			_pageManagerMock.Setup(x => x.GetById(It.IsAny<int>())).Returns<int>(x =>
				{
					PageContent content = _pagesContent.FirstOrDefault(p => p.Page.Id == x);

					if (content != null)
						return content.ToSummary(_markupConverter);
					else
						return null;
				});
			_pageManagerMock.Setup(x => x.FindByTag(It.IsAny<string>()));
			_pageManager = _pageManagerMock.Object;

			_pagesController = new PagesController(_config, _userManager, _settingsManager, _pageManager, _searchManager, _historyManager, _contextStub);
			_mocksContainer = _pagesController.SetFakeControllerContext();
		}

		private Page AddDummyPage1()
		{
			Page page1 = new Page() { Id = 1, Tags = "tag1,tag2", Title = "Welcome to the site", CreatedBy = "admin" };
			PageContent page1Content = new PageContent() { Id = Guid.NewGuid(), Page = page1, Text = "Hello world 1", VersionNumber = 1 };
			_pages.Add(page1);
			_pagesContent.Add(page1Content);

			return page1;
		}

		private Page AddDummyPage2()
		{
			Page page2 = new Page() { Id = 50, Tags = "anothertag", Title = "Page 2" };
			PageContent page2Content = new PageContent() { Id = Guid.NewGuid(), Page = page2, Text = "Hello world 2" };
			_pages.Add(page2);
			_pagesContent.Add(page2Content);

			return page2;
		}

		[Test]
		public void AllPages_Should_Return_Model_And_Pages()
		{
			// Arrange
			Page page1 = AddDummyPage1();
			Page page2 = AddDummyPage2();
			PageContent page1Content = _pagesContent.First(p => p.Page.Id == page1.Id);

			// Act
			ActionResult result = _pagesController.AllPages();

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			IEnumerable<PageSummary> model = result.ModelFromActionResult<IEnumerable<PageSummary>>();
			Assert.NotNull(model, "Null model");

			List<PageSummary> summaryList = model.OrderBy(p => p.Id).ToList();
			_pageManagerMock.Verify(x => x.AllPages(false));
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

			IEnumerable<TagSummary> model = result.ModelFromActionResult<IEnumerable<TagSummary>>();
			Assert.NotNull(model, "Null model");
			_pageManagerMock.Verify(x => x.AllTags());
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

			Assert.That(jsonResult.JsonRequestBehavior, Is.EqualTo(JsonRequestBehavior.AllowGet));
			Assert.That(jsonResult.Data, Is.TypeOf<Dictionary<string, object>>());
			_pageManagerMock.Verify(x => x.AllTags());
		}

		[Test]
		public void ByUser_Should_Contains_ViewData_And_Return_Model_And_Pages()
		{
			// Arrange
			string username = "amazinguser";

			Page page1 = AddDummyPage1();
			page1.CreatedBy = username;
			PageContent page1Content = _pagesContent.First(p => p.Page.Id == page1.Id);

			Page page2 = AddDummyPage2();
			page2.CreatedBy = username;

			// Act
			ActionResult result = _pagesController.ByUser(username, false);

			// Assert
			Assert.That(_pagesController.ViewData.Keys.Count, Is.GreaterThanOrEqualTo(1));

			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			IEnumerable<PageSummary> model = result.ModelFromActionResult<IEnumerable<PageSummary>>();
			Assert.NotNull(model, "Null model");
			_pageManagerMock.Verify(x => x.AllPagesCreatedBy(username));
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
			_pageManagerMock.Verify(x => x.DeletePage(50));
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
			_pageManagerMock.Setup(x => x.GetById(page.Id)).Returns(new PageSummary() { Id = page.Id, IsLocked = true });

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
			PageContent pageContent = _pagesContent.First(p => p.Page.Id == page.Id);
			_pageManagerMock.Setup(x => x.GetById(page.Id)).Returns(new PageSummary() { Id = page.Id,});

			// Act
			ActionResult result = _pagesController.Edit(page.Id);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			ViewResult viewResult = result as ViewResult;
			Assert.NotNull(viewResult, "Null viewResult");
			_pageManagerMock.Verify(x => x.GetById(page.Id));
		}

		[Test]
		public void Edit_POST_Should_Return_RedirectResult_And_Call_PageManager()
		{
			// Arrange
			_contextStub.CurrentUser = "Admin";
			Page page = AddDummyPage1();
			PageContent pageContent = _pagesContent[0];

			PageSummary summary = new PageSummary();
			summary.Id = page.Id;
			summary.RawTags = "newtag1,newtag2";
			summary.Title = "New page title";
			summary.Content = "*Some new content here*";

			// Act
			ActionResult result = _pagesController.Edit(summary);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult");
			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.NotNull(redirectResult, "Null RedirectToRouteResult");

			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Wiki"));
			Assert.That(_pages.Count, Is.EqualTo(1));
			_pageManagerMock.Verify(x => x.UpdatePage(summary));
		}

		[Test]
		public void Edit_POST_With_Invalid_Data_Should_Return_View_And_Invalid_ModelState()
		{
			// Arrange
			_contextStub.CurrentUser = "Admin";
			Page page = AddDummyPage1();

			PageSummary summary = new PageSummary();
			summary.Id = page.Id;

			// Act
			_pagesController.ModelState.AddModelError("Title", "You forgot it");
			ActionResult result = _pagesController.Edit(summary);

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
			ActionResult result = _pagesController.GetPreview(_pagesContent[0].Text);

			// Assert
			_pageManagerMock.Verify(x => x.GetMarkupConverter());

			Assert.That(result, Is.TypeOf<JavaScriptResult>(), "JavaScriptResult");
			JavaScriptResult javascriptResult = result as JavaScriptResult;
			Assert.That(javascriptResult.Script, Contains.Substring(_pagesContent[0].Text));
		}

		[Test]
		public void History_Returns_ViewResult_And_Model_With_Two_Versions()
		{
			// Arrange
			Page page = AddDummyPage1();
			_pagesContent.Add(new PageContent() { VersionNumber = 2, Page = page, Id = Guid.NewGuid(), Text = "v2text" });

			// Act
			ActionResult result = _pagesController.History(page.Id);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			ViewResult viewResult = result as ViewResult;

			List<HistorySummary> model = viewResult.ModelFromActionResult<IEnumerable<HistorySummary>>().ToList();
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
			
			PageSummary summary = viewResult.ModelFromActionResult<PageSummary>();
			Assert.NotNull(summary, "Null model");
			Assert.That(summary.Title, Is.EqualTo(title));
		}

		[Test]
		public void New_POST_Should_Return_RedirectResult_And_Call_PageManager()
		{
			// Arrange
			PageSummary summary = new PageSummary();
			summary.RawTags = "newtag1,newtag2";
			summary.Title = "New page title";
			summary.Content = "*Some new content here*";

			_pageManagerMock.Setup(x => x.AddPage(summary)).Returns(() =>
			{
				_pages.Add(new Page() { Id = 50, Title = summary.Title, Tags = summary.RawTags });
				_pagesContent.Add(new PageContent() { Id = Guid.NewGuid(), Text = summary.Content });
				summary.Id = 50;

				return summary;
			});

			// Act
			ActionResult result = _pagesController.New(summary);

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "RedirectToRouteResult not returned");
			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.NotNull(redirectResult, "Null RedirectToRouteResult");

			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Wiki"));
			Assert.That(_pages.Count, Is.EqualTo(1));
			_pageManagerMock.Verify(x => x.AddPage(summary));
		}

		[Test]
		public void New_POST_With_Invalid_Data_Should_Return_View_And_Invalid_ModelState()
		{
			// Arrange
			PageSummary summary = new PageSummary();
			summary.Title = "";

			// Act
			_pagesController.ModelState.AddModelError("Title", "You forgot it");
			ActionResult result = _pagesController.New(summary);

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

			_pagesContent.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text"});
			_pagesContent.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text"});

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

			_pagesContent.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text" });
			_pagesContent.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text" });

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
		public void Tag_Returns_ViewResult_And_Calls_PageManager()
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
			_pageManagerMock.Verify(x => x.FindByTag("tag2"));
		}

		[Test]
		public void Version_Should_Return_ViewResult_And_PageSummary_Model()
		{
			// Arrange
			Page page = AddDummyPage1();
			page.IsLocked = true;

			Guid version2Guid = Guid.NewGuid();
			Guid version3Guid = Guid.NewGuid();

			_pagesContent.Add(new PageContent() { Id = version2Guid, Page = page, Text = "version2 text" });
			_pagesContent.Add(new PageContent() { Id = version3Guid, Page = page, Text = "version3 text" });

			// Act
			ActionResult result = _pagesController.Version(version2Guid);

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");
			ViewResult viewResult = result as ViewResult;
			Assert.NotNull(viewResult, "Null ViewResult");

			PageSummary summary = viewResult.ModelFromActionResult<PageSummary>();
			Assert.That(summary.Content, Contains.Substring("version2 text"));
		}
	}
}
