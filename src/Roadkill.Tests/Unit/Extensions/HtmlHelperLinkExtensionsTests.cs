using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Extensions;
using Roadkill.Core.Mvc;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using StructureMap;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class HtmlHelperLinkExtensionsTests
	{
		// Objects for the HtmlHelper
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
		private HtmlHelper _htmlHelper;
		private ViewContext _viewContext;

		[SetUp]
		public void Setup()
		{
			// WikiController setup (use WikiController as it's the one typically used by views)
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
			_wikiController.SetFakeControllerContext("~/wiki/index/1");

			// HtmlHelper setup
			var viewDataDictionary = new ViewDataDictionary();
			_viewContext = new ViewContext(_wikiController.ControllerContext, new Mock<IView>().Object, viewDataDictionary, new TempDataDictionary(), new StringWriter());
			var mockViewDataContainer = new Mock<IViewDataContainer>();
			mockViewDataContainer.Setup(v => v.ViewData).Returns(viewDataDictionary);

			_htmlHelper = new HtmlHelper(_viewContext, mockViewDataContainer.Object);
		}

		[Test]
		public void LoginStatus_Should_Contain_Link_And_Username_In_Text_When_User_Is_LoggedIn()
		{
			// Arrange
			_context.CurrentUser = "editor";
			string expectedHtml = "<a href=\"/user/profile\">Logged in as editor</a>";

			// Act
			string actualHtml = _htmlHelper.LoginStatus().ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void LoginStatus_Should_Contain_Login_Link_And_Guest_In_Text_When_User_Is_Anonymous()
		{
			// Arrange
			_context.CurrentUser = "";
			string expectedHtml = "Not logged in";

			// Act
			string actualHtml = _htmlHelper.LoginStatus().ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void SettingsLink_Should_Render_Link_Html_When_Logged_In_As_Admin()
		{
			// Arrange
			_userService.AddUser("admin@localhost", "admin", "password", true, true);
			Guid userId = _userService.ListAdmins().First().Id.Value;
			_context.CurrentUser = userId.ToString();

			string expectedHtml = "@<a href=\"/settings\">Site settings</a>~";

			// Act
			string actualHtml = _htmlHelper.SettingsLink("@","~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void SettingsLink_Should_Not_Render_Html_When_Logged_In_As_Editor()
		{
			// Arrange
			_userService.AddUser("editor@localhost", "admin", "password", false, true);
			Guid userId = _userService.ListEditors().First().Id.Value;
			_context.CurrentUser = userId.ToString();

			string expectedHtml = "";

			// Act
			string actualHtml = _htmlHelper.SettingsLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void SettingsLink_Should_Not_Render_Html_When_Anonymous_User()
		{
			// Arrange
			_context.CurrentUser = "";
			string expectedHtml = "";

			// Act
			string actualHtml = _htmlHelper.SettingsLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void FileManagerLink_Should_Render_Link_Html_When_Logged_In_As_Admin()
		{
			// Arrange
			_userService.AddUser("admin@localhost", "admin", "password", true, true);
			Guid userId = _userService.ListAdmins().First().Id.Value;
			_context.CurrentUser = userId.ToString();

			string expectedHtml = "@<a href=\"/filemanager\">Manage files</a>~";

			// Act
			string actualHtml = _htmlHelper.FileManagerLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void FileManagerLink_Should_Render_Link_Html_When_Logged_In_As_Editor()
		{
			// Arrange
			_userService.AddUser("editor@localhost", "editor", "password", false, true);
			Guid userId = _userService.ListEditors().First().Id.Value;
			_context.CurrentUser = userId.ToString();

			string expectedHtml = "@<a href=\"/filemanager\">Manage files</a>~";

			// Act
			string actualHtml = _htmlHelper.FileManagerLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void FileManagerLink_Should_Not_Render_Html_When_Anonymous_User()
		{
			// Arrange
			_context.CurrentUser = "";
			string expectedHtml = "";

			// Act
			string actualHtml = _htmlHelper.FileManagerLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void LoginLink_Should_Render_Login_Html_When_Anonymous_User()
		{
			// Arrange
			_context.CurrentUser = "";
			string expectedHtml = "@<a href=\"/user/login?returnurl=~%2fwiki%2findex%2f1\">Login</a>~";

			// Act
			string actualHtml = _htmlHelper.LoginLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void LoginLink_Should_Render_Logout_Html_When_Logged_In_As_Editor()
		{
			// Arrange
			_userService.AddUser("editor@localhost", "editor", "password", false, true);
			Guid userId = _userService.ListEditors().First().Id.Value;
			_context.CurrentUser = userId.ToString();

			string expectedHtml = "@<a href=\"/user/logout\">Logout</a>~";

			// Act
			string actualHtml = _htmlHelper.LoginLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void LoginLink_Should_Return_Empty_String_When_Windows_Auth_Is_Enabled()
		{
			// Arrange
			_applicationSettings.UseWindowsAuthentication = true;
			string expectedHtml = "";

			// Act
			string actualHtml = _htmlHelper.LoginLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void NewPageLink_Should_Render_Link_Html_When_Logged_In_As_Admin()
		{
			// Arrange
			_userService.AddUser("admin@localhost", "admin", "password", true, true);
			Guid userId = _userService.ListAdmins().First().Id.Value;
			_context.CurrentUser = userId.ToString();

			string expectedHtml = "@<a href=\"/pages/new\">New page</a>~";

			// Act
			string actualHtml = _htmlHelper.NewPageLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void NewPageLink_Should_Render_Link_Html_When_Logged_In_As_Editor()
		{
			// Arrange
			_userService.AddUser("editor@localhost", "editor", "password", false, true);
			Guid userId = _userService.ListEditors().First().Id.Value;
			_context.CurrentUser = userId.ToString();

			string expectedHtml = "@<a href=\"/pages/new\">New page</a>~";

			// Act
			string actualHtml = _htmlHelper.NewPageLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void NewPageLink_Should_Not_Render_Html_When_Anonymous_User()
		{
			// Arrange
			_context.CurrentUser = "";
			string expectedHtml = "";

			// Act
			string actualHtml = _htmlHelper.NewPageLink("@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void MainPageLink_Should_Render_Html_With_Home_Link()
		{
			// Arrange
			string expectedHtml = "@<a href=\"/\">the fun starts here</a>~";

			// Act
			string actualHtml = _htmlHelper.MainPageLink("the fun starts here", "@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}


		[Test]
		public void PageLink_Should_Render_Html_Link_With_Page_Title_And_Html_Attributes()
		{
			// Arrange
			_pageService.AddPage(new PageViewModel() { Id = 7, Title = "Crispy Pancake Recipe" });
			ObjectFactory.Configure(x => x.For<IPageService>().Use(_pageService)); // the extension uses bastard injection

			string expectedHtml = "@<a data-merry=\"xmas\" href=\"/wiki/1/crispy%20pancake%20recipe\">captains log</a>~"; // the url will always be /wiki/1 because of the mock url setup

			// Act
			string actualHtml = _htmlHelper.PageLink("captains log", "Crispy Pancake Recipe", new { data_merry = "xmas" }, "@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void PageLink_Should_Render_Html_With_No_Link_When_Page_Does_Not_Exist()
		{
			// Arrange
			ObjectFactory.Configure(x => x.For<IPageService>().Use(_pageService)); // the extension uses bastard injection

			string expectedHtml = "captains log"; // the url will always be /wiki/1 because of the mock url setup

			// Act
			string actualHtml = _htmlHelper.PageLink("captains log", "Random page that doesnt exist", new { data_merry = "xmas" }, "@", "~").ToString();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
