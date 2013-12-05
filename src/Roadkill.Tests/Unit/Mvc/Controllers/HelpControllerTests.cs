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
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private SettingsService _settingsService;

		private HelpController _helpController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_pageService = _container.PageService;

			_helpController = new HelpController(_applicationSettings, _userService, _context, _settingsService, _pageService);
		}

		[Test]
		public void Index_Should_Return_ViewResult()
		{
			// Arrange
			_repository.SiteSettings.MarkupType = "Mediawiki";

			// Act
			ViewResult result = _helpController.Index() as ViewResult;

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
			ViewResult result = _helpController.About() as ViewResult;

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
			RedirectToRouteResult result = _helpController.About() as RedirectToRouteResult;

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
			ViewResult result = _helpController.CreoleReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void MediaWikiReference_Should_Return_View()
		{
			// Arrange


			// Act
			ViewResult result = _helpController.MediaWikiReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void MarkdownReference_Should_Return_View()
		{
			// Arrange


			// Act
			ViewResult result = _helpController.MarkdownReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}
	}
}
