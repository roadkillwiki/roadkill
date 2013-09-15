using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Text;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class MenuParserTests
	{
		[Test]
		public void Should_Replace_Known_Tokens_When_Logged_In_As_Admin()
		{
			// Arrange
			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
			string expectedHtml = "<ul>\n<li><a href=\"/pages/alltags\">Categories</a></li>\n</ul>\n\n" +
								  "<p><a href=\"/pages/allpages\">All pages</a>\n" +
								  "<a href=\"/\">Main Page</a>\n" +
								  "<a href=\"/pages/new\">New page</a>\n" +
								  "<a href=\"/filemanager\">Manage files</a>\n" +
								  "<a href=\"/settings\">Site settings</a></p>\n";

			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings = new SiteSettings();
			repository.SiteSettings.MarkupType = "Markdown";
			repository.SiteSettings.MenuMarkup = menuMarkup;

			RoadkillContextStub userContext = new RoadkillContextStub();
			userContext.IsAdmin = true;
			userContext.IsLoggedIn = true;

			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.Installed = true;
			CacheMock cache = new CacheMock();
			ListCache listCache = new ListCache(applicationSettings, cache);

			MarkupConverter converter = new MarkupConverter(applicationSettings, repository);
			MenuParser parser = new MenuParser(converter, repository, listCache, userContext);

			// Act
			string actualHtml = parser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Should_Replace_Known_Tokens_When_Logged_In_As_Editor()
		{
			// Arrange
			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
			string expectedHtml = "<ul>\n<li><a href=\"/pages/alltags\">Categories</a></li>\n</ul>\n\n" +
								  "<p><a href=\"/pages/allpages\">All pages</a>\n" +
								  "<a href=\"/\">Main Page</a>\n" +
								  "<a href=\"/pages/new\">New page</a>\n<a href=\"/filemanager\">Manage files</a>\n</p>\n";

			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings = new SiteSettings();
			repository.SiteSettings.MarkupType = "Markdown";
			repository.SiteSettings.MenuMarkup = menuMarkup;

			RoadkillContextStub userContext = new RoadkillContextStub();
			userContext.IsAdmin = false;
			userContext.IsLoggedIn = true;

			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.Installed = true;
			CacheMock cache = new CacheMock();
			ListCache listCache = new ListCache(applicationSettings, cache);

			MarkupConverter converter = new MarkupConverter(applicationSettings, repository);
			MenuParser parser = new MenuParser(converter, repository, listCache, userContext);

			// Act
			string actualHtml = parser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void Should_Replace_Known_Tokens_When_Not_Logged()
		{
			// Arrange
			string menuMarkup = "* %categories%\r\n\r\n%allpages%\r\n%mainpage%\r\n%newpage%\r\n%managefiles%\r\n%sitesettings%\r\n";
			string expectedHtml = "<ul>\n<li><a href=\"/pages/alltags\">Categories</a></li>\n</ul>\n\n" +
								  "<p><a href=\"/pages/allpages\">All pages</a>\n" +
								  "<a href=\"/\">Main Page</a>\n\n\n</p>\n";

			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings = new SiteSettings();
			repository.SiteSettings.MarkupType = "Markdown";
			repository.SiteSettings.MenuMarkup = menuMarkup;

			RoadkillContextStub userContext = new RoadkillContextStub();
			userContext.IsLoggedIn = false;

			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.Installed = true;
			CacheMock cache = new CacheMock();
			ListCache listCache = new ListCache(applicationSettings, cache);

			MarkupConverter converter = new MarkupConverter(applicationSettings, repository);
			MenuParser parser = new MenuParser(converter, repository, listCache, userContext);

			// Act
			string actualHtml = parser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		[TestCase("Creole", "<p><a href=\"/\">Main Page</a>\n</p>\n")]
		[TestCase("Markdown", "<p><a href=\"/\">Main Page</a></p>\n\n")]
		public void Should_Remove_Empty_UL_Tags_For_Logged_In_Tokens_When_Not_Logged_In(string markupType, string expectedHtml)
		{
			// Arrange - \r\n is important so the markdown is valid
			string menuMarkup = "%mainpage%\r\n\r\n* %newpage%\r\n* %managefiles%\r\n* %sitesettings%\r\n";

			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings = new SiteSettings();
			repository.SiteSettings.MarkupType = markupType;
			repository.SiteSettings.MenuMarkup = menuMarkup;

			RoadkillContextStub userContext = new RoadkillContextStub();
			userContext.IsLoggedIn = false;

			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.Installed = true;
			CacheMock cache = new CacheMock();
			ListCache listCache = new ListCache(applicationSettings, cache);

			MarkupConverter converter = new MarkupConverter(applicationSettings, repository);
			MenuParser parser = new MenuParser(converter, repository, listCache, userContext);

			// Act
			string actualHtml = parser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Should_Cache_Menu_Html_For_Admin_And_Editor_And_Guest_User()
		{
			// Arrange
			string menuMarkup = "My menu %newpage% %sitesettings%";

			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings = new SiteSettings();
			repository.SiteSettings.MarkupType = "Markdown";
			repository.SiteSettings.MenuMarkup = menuMarkup;

			RoadkillContextStub userContext = new RoadkillContextStub();
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.Installed = true;

			CacheMock cache = new CacheMock();
			ListCache listCache = new ListCache(applicationSettings, cache);

			MarkupConverter converter = new MarkupConverter(applicationSettings, repository);
			MenuParser parser = new MenuParser(converter, repository, listCache, userContext);

			// Act
			userContext.IsLoggedIn = false;
			userContext.IsAdmin = false;
			parser.GetMenu();

			userContext.IsLoggedIn = true;
			userContext.IsAdmin = false;
			parser.GetMenu();

			userContext.IsLoggedIn = true;
			userContext.IsAdmin = true;
			parser.GetMenu();

			// Assert
			Assert.That(cache.CacheItems.Count, Is.EqualTo(3));
		}

		[Test]
		public void Should_Return_Different_Menu_Html_For_Admin_And_Editor_And_Guest_User()
		{
			// Arrange
			string menuMarkup = "My menu %newpage% %sitesettings%";

			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings = new SiteSettings();
			repository.SiteSettings.MarkupType = "Markdown";
			repository.SiteSettings.MenuMarkup = menuMarkup;

			RoadkillContextStub userContext = new RoadkillContextStub();
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.Installed = true;

			CacheMock cache = new CacheMock();
			ListCache listCache = new ListCache(applicationSettings, cache);

			MarkupConverter converter = new MarkupConverter(applicationSettings, repository);
			MenuParser parser = new MenuParser(converter, repository, listCache, userContext);

			// Act
			userContext.IsLoggedIn = false;
			userContext.IsAdmin = false;
			string guestHtml = parser.GetMenu();

			userContext.IsLoggedIn = true;
			userContext.IsAdmin = false;
			string editorHtml = parser.GetMenu();

			userContext.IsLoggedIn = true;
			userContext.IsAdmin = true;
			string adminHtml = parser.GetMenu();

			// Assert
			Assert.That(guestHtml, Is.EqualTo("<p>My menu  </p>\n"));
			Assert.That(editorHtml, Is.EqualTo("<p>My menu <a href=\"/pages/new\">New page</a> </p>\n"));
			Assert.That(adminHtml, Is.EqualTo("<p>My menu <a href=\"/pages/new\">New page</a> <a href=\"/settings\">Site settings</a></p>\n"));
		}
		
		[Test]
		public void Should_Replace_Markdown_With_External_Link()
		{
			// Arrange
			string menuMarkup = "* [First link](http://www.google.com)\r\n";
			string expectedHtml = "<ul>\n<li><a href=\"http://www.google.com\">First link</a></li>\n</ul>\n";

			RepositoryMock repository = new RepositoryMock();
			repository.SiteSettings = new SiteSettings();
			repository.SiteSettings.MarkupType = "Markdown";
			repository.SiteSettings.MenuMarkup = menuMarkup;

			RoadkillContextStub userContext = new RoadkillContextStub();
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.Installed = true;

			CacheMock cache = new CacheMock();
			ListCache listCache = new ListCache(applicationSettings, cache);

			MarkupConverter converter = new MarkupConverter(applicationSettings, repository);
			MenuParser parser = new MenuParser(converter, repository, listCache, userContext);

			// Act
			string actualHtml = parser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}

		[Test]
		public void Should_Replace_Markdown_With_Internal_Link()
		{
			// Arrange
			string menuMarkup = "* [First link](my-page)\r\n";
			string expectedHtml = "<ul>\n<li><a href=\"/wiki/1/my-page\">First link</a></li>\n</ul>\n";

			RepositoryMock repository = new RepositoryMock();
			repository.AddNewPage(new Page() { Title = "my page", Id = 1 }, "text", "user", DateTime.Now);

			repository.SiteSettings = new SiteSettings();
			repository.SiteSettings.MarkupType = "Markdown";
			repository.SiteSettings.MenuMarkup = menuMarkup;

			RoadkillContextStub userContext = new RoadkillContextStub();
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.Installed = true;

			CacheMock cache = new CacheMock();
			ListCache listCache = new ListCache(applicationSettings, cache);

			MarkupConverter converter = new MarkupConverter(applicationSettings, repository);
			MenuParser parser = new MenuParser(converter, repository, listCache, userContext);

			// Act
			string actualHtml = parser.GetMenu();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml));
		}
	}
}
