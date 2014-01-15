using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Tests.Unit.WebApi
{
	[TestFixture]
	[Category("Unit")]
	public class PageControllerTests
	{
		private MocksAndStubsContainer _container;

		private RepositoryMock _repositoryMock;
		private ApplicationSettings _applicationSettings;
		private UserServiceMock _userService;
		private IUserContext _userContext;
		private PageService _pageService;
		private PagesController _pagesController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_userContext = _container.UserContext;
			_userService = _container.UserService;
			_repositoryMock = _container.Repository;
			_pageService = _container.PageService;

			_pagesController = new PagesController(_pageService, _applicationSettings, _userService, _userContext);
		}

		[Test]
		public void Get_Should_Return_All_Pages()
		{
			// Arrange
			_pageService.AddPage(new PageViewModel() { Id = 1, Title = "new page" });
			_pageService.AddPage(new PageViewModel() { Id = 2, Title = "new page", IsLocked = true });

			// Act
			IEnumerable<PageViewModel> pages = _pagesController.Get();

			// Assert
			Assert.That(pages.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Get_Should_Return_Page_By_Id()
		{
			// Arrange
			Page expectedPage = new Page() { Id = 7, Title = "new page" };
			_repositoryMock.Pages.Add(expectedPage);

			// Act
			PageViewModel actualPage = _pagesController.Get(7);

			// Assert
			Assert.That(actualPage, Is.Not.Null);
			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Id));
		}

		[Test]
		public void Get_Should_Return_Null_When_Page_Does_Not_Exist()
		{
			// Arrange

			// Act
			PageViewModel actualPage = _pagesController.Get(99);

			// Assert
			Assert.That(actualPage, Is.Null);
		}

		[Test]
		public void Put_Should_Update_Page()
		{
			// Arrange
			DateTime version1Date = DateTime.Today.AddDays(-1); // stops the getlatestcontent acting up when add+update are the same time

			Page page = new Page();
			page.Title = "Hello world";
			page.Tags = "tag1, tag2";
			page.CreatedOn = version1Date;
			page.ModifiedOn = version1Date;
			PageContent pageContent = _repositoryMock.AddNewPage(page, "Some content1", "editor", version1Date);

			PageViewModel model = new PageViewModel(pageContent.Page);
			model.Title = "New title";
			model.Content = "Some content2";
			model.ModifiedOn = DateTime.UtcNow;

			// Act
			_pagesController.Put(model);

			// Assert
			 Assert.That(_pageService.AllPages().Count(), Is.EqualTo(1));

			PageViewModel actualPage = _pageService.GetById(1, true);
			Assert.That(actualPage.Title, Is.EqualTo("New title"));
			Assert.That(actualPage.Content, Is.EqualTo("Some content2"));
		}

		[Test]
		public void Post_Should_Add_Page()
		{
			// Arrange
			PageViewModel model = new PageViewModel();
			model.Title = "Hello world";
			model.RawTags = "tag1, tag2";
			model.Content = "Some content";

			// Act
			_pagesController.Post(model);

			// Assert
			Assert.That(_pageService.AllPages().Count(), Is.EqualTo(1));
		}
	}
}