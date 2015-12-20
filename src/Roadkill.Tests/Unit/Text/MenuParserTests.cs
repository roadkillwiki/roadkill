using System;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Text;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Text
{
	[TestFixture]
	[Category("Unit")]
	public class MenuParserTests
	{
		private PluginFactoryMock _pluginFactory;
		private PageRepositoryMock _pageRepository;
		private SettingsRepositoryMock _settingsRepository;
		private UserContextStub _userContext;
		private ApplicationSettings _applicationSettings;
		private CacheMock _cache;
		private SiteCache _siteCache;
		private MarkupConverter _converter;
		private MenuParser _menuParser;

		[SetUp]
		public void Setup()
		{
			_pluginFactory = new PluginFactoryMock();

			_pageRepository = new PageRepositoryMock();

			_settingsRepository = new SettingsRepositoryMock();
			_settingsRepository.SiteSettings = new SiteSettings();
			_settingsRepository.SiteSettings.MarkupType = "Markdown";

			_userContext = new UserContextStub();

			_applicationSettings = new ApplicationSettings();
			_applicationSettings.Installed = true;

			_cache = new CacheMock();
			_siteCache = new SiteCache(_cache);

			_converter = new MarkupConverter(_applicationSettings, _settingsRepository, _pageRepository, _pluginFactory);
			_menuParser = new MenuParser(_converter, _settingsRepository, _siteCache, _userContext);
		}

		[Test]
		public void should_replace_known_tokens_when_logged_in_as_admin()
		{
			// Arrange
			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
			string expectedHtml = "<ul><li><a href=\"/pages/alltags\">Categories</a></li></ul>" +
								  "<a href=\"/pages/allpages\">All pages</a>" +
								  "<a href=\"/\">Main Page</a>" +
								  "<a href=\"/pages/new\">New page</a>" +
								  "<a href=\"/filemanager\">Manage files</a>" +
								  "<a href=\"/settings\">Site settings</a>";

			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

			_userContext.IsAdmin = true;
			_userContext.IsLoggedIn = true;

			// Act
			string actualHtml = _menuParser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void should_replace_known_tokens_when_logged_in_as_editor()
		{
			// Arrange
			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
			string expectedHtml = "<ul><li><a href=\"/pages/alltags\">Categories</a></li></ul>" +
								  "<a href=\"/pages/allpages\">All pages</a>" +
								  "<a href=\"/\">Main Page</a>" +
								  "<a href=\"/pages/new\">New page</a><a href=\"/filemanager\">Manage files</a>";

			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

			_userContext.IsAdmin = false;
			_userContext.IsLoggedIn = true;

			// Act
			string actualHtml = _menuParser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void should_replace_known_tokens_when_not_logged()
		{
			// Arrange
			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
			string expectedHtml = "<ul><li><a href=\"/pages/alltags\">Categories</a></li></ul>" +
								  "<a href=\"/pages/allpages\">All pages</a>" +
								  "<a href=\"/\">Main Page</a>";

			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

			// Act
			string actualHtml = _menuParser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		[TestCase("Creole", "<a href=\"/\">Main Page</a>")]
		[TestCase("Markdown", "<a href=\"/\">Main Page</a>")]
		public void Should_Remove_Empty_UL_Tags_For_Logged_In_Tokens_When_Not_Logged_In(string markupType, string expectedHtml)
		{
			// Arrange - \r\n is important so the markdown is valid
			string menuMarkup = "%mainpage%\r\n\r\n* %newpage%\r\n* %managefiles%\r\n* %sitesettings%\r\n";
			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

			// Act
			string actualHtml = _menuParser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_cache_menu_html_for_admin_and_editor_and_guest_user()
		{
			// Arrange
			string menuMarkup = "My menu %newpage% %sitesettings%";
			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

			// Act
			_userContext.IsLoggedIn = false;
			_userContext.IsAdmin = false;
			_menuParser.GetMenu();

			_userContext.IsLoggedIn = true;
			_userContext.IsAdmin = false;
			_menuParser.GetMenu();

			_userContext.IsLoggedIn = true;
			_userContext.IsAdmin = true;
			_menuParser.GetMenu();

			// Assert
			Assert.That(_cache.CacheItems.Count, Is.EqualTo(3));
		}

		[Test]
		public void should_return_different_menu_html_for_admin_and_editor_and_guest_user()
		{
			// Arrange
			string menuMarkup = "My menu %newpage% %sitesettings%";
			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

			// Act
			_userContext.IsLoggedIn = false;
			_userContext.IsAdmin = false;
			string guestHtml = _menuParser.GetMenu();

			_userContext.IsLoggedIn = true;
			_userContext.IsAdmin = false;
			string editorHtml = _menuParser.GetMenu();

			_userContext.IsLoggedIn = true;
			_userContext.IsAdmin = true;
			string adminHtml = _menuParser.GetMenu();

			// Assert
			Assert.That(guestHtml, Is.EqualTo("My menu"));
			Assert.That(editorHtml, Is.EqualTo("My menu <a href=\"/pages/new\">New page</a>"));
			Assert.That(adminHtml, Is.EqualTo("My menu <a href=\"/pages/new\">New page</a> <a href=\"/settings\">Site settings</a>"));
		}
		
		[Test]
		public void should_replace_markdown_with_external_link()
		{
			// Arrange
			string menuMarkup = "* [First link](http://www.google.com)\r\n";
			string expectedHtml = "<ul><li><a href=\"http://www.google.com\" rel=\"nofollow\" class=\"external-link\">First link</a></li></ul>";
			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

			// Act
			string actualHtml = _menuParser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void should_replace_markdown_with_internal_link()
		{
			// Arrange
			string menuMarkup = "* [First link](my-page)\r\n";
			string expectedHtml = "<ul><li><a href=\"/wiki/1/my-page\">First link</a></li></ul>";
			_settingsRepository.SiteSettings.MenuMarkup = menuMarkup;

			_pageRepository.AddNewPage(new Page() { Title = "my page", Id = 1 }, "text", "user", DateTime.Now);

			// Act
			string actualHtml = _menuParser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
