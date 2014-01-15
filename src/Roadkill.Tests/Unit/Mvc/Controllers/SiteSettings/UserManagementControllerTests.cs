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
using System.Collections.Generic;
using System.Runtime.Caching;
using Roadkill.Core.Import;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class UserManagementControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private IWikiImporter _wikiImporter;
		private PluginFactoryMock _pluginFactory;
		private SearchService _searchService;
		private SettingsService _settingsService;
		private PageViewModelCache _pageCache;
		private ListCache _listCache;
		private SiteCache _siteCache;
		private MemoryCache _cache;

		private UserManagementController _controller;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_pageCache = _container.PageViewModelCache;
			_listCache = _container.ListCache;
			_siteCache = _container.SiteCache;
			_cache = _container.MemoryCache;

			_pageService = _container.PageService;
			_wikiImporter = new ScrewTurnImporter(_applicationSettings, _repository);
			_pluginFactory = _container.PluginFactory;
			_searchService = _container.SearchService;

			_controller = new UserManagementController(_applicationSettings, _userService, _settingsService, _pageService, 
				_searchService, _context, _listCache, _pageCache, _siteCache, _wikiImporter, _repository, _pluginFactory);
		}

		[Test]
		public void AddAdmin_GET_Should_Return_View_And_ViewModel()
		{
			// Arrange

			// Act
			ViewResult result = _controller.AddAdmin() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			UserViewModel model = result.ModelFromActionResult<UserViewModel>();
			Assert.That(model, Is.Not.Null, "model");
		}

		[Test]
		public void AddAdmin_POST_Should_Add_Admin_And_Redirect_To_Index()
		{
			// Arrange
			UserViewModel model = new UserViewModel();

			// Act
			RedirectToRouteResult result = _controller.AddAdmin(model) as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(_userService.Users.Count, Is.EqualTo(1));
		}

		[Test]
		public void AddAdmin_POST_Should_Return_ViewResult_When_ModelState_Is_Invalid()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			_controller.ModelState.AddModelError("username", "notvalid"); // force an error

			// Act
			ViewResult result = _controller.AddAdmin(model) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			UserViewModel resultModel = result.ModelFromActionResult<UserViewModel>();
			Assert.That(resultModel, Is.Not.Null, "resultModel");
		}

		[Test]
		public void AddEditor_GET_Should_Return_View_And_ViewModel()
		{
			// Arrange

			// Act
			ViewResult result = _controller.AddEditor() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			UserViewModel model = result.ModelFromActionResult<UserViewModel>();
			Assert.That(model, Is.Not.Null, "model");
		}

		[Test]
		public void AddEditor_POST_Should_Add_Admin_And_Redirect_To_Index()
		{
			// Arrange
			UserViewModel model = new UserViewModel();

			// Act
			RedirectToRouteResult result = _controller.AddEditor(model) as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(_userService.Users.Count, Is.EqualTo(1));
		}

		[Test]
		public void AddEditor_POST_Should_Return_ViewResult_When_ModelState_Is_Invalid()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			_controller.ModelState.AddModelError("username", "notvalid"); // force an error

			// Act
			ViewResult result = _controller.AddEditor(model) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			UserViewModel resultModel = result.ModelFromActionResult<UserViewModel>();
			Assert.That(resultModel, Is.Not.Null, "resultModel");
		}

		[Test]
		public void DeleteUser_Should_Remove_User_And_Redirect_To_Index()
		{
			// Arrange
			User user = new User() { Id = Guid.NewGuid(), Email="blah@localhost", IsActivated = true };
			_userService.Users.Add(user);

			// Act
			RedirectToRouteResult result = _controller.DeleteUser(user.Email) as RedirectToRouteResult;

			// Assert
			Assert.That(_userService.Users.Count, Is.EqualTo(0));
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
		}

		[Test]
		public void EditUser_GET_Should_Return_View_And_ViewModel()
		{
			// Arrange
			User user = new User() { Id = Guid.NewGuid(), IsActivated = true };
			_userService.Users.Add(user);

			// Act
			ViewResult result = _controller.EditUser(user.Id) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			UserViewModel model = result.ModelFromActionResult<UserViewModel>();
			Assert.That(model, Is.Not.Null, "model");
			Assert.That(model.Id, Is.EqualTo(user.Id), "model");
		}

		[Test]
		public void EditUser_GET_Should_Redirect_When_User_Does_Not_Exist()
		{
			// Arrange

			// Act
			RedirectToRouteResult result = _controller.EditUser(Guid.NewGuid()) as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
		}

		[Test]
		public void EditUser_POST_Should_Update_User_When_Username_And_Email_Changes_And_Redirect_To_Index()
		{
			// Arrange
			User user = new User()
			{ 
				Id = Guid.NewGuid(), 
				IsActivated = true, 
				Lastname = "Lastname",
				Firstname = "Firstname",
				Email = "email@localhost",
				Username = "username"
			};
			_userService.Users.Add(user);

			UserViewModel model = new UserViewModel(user);
			model.Lastname = "new lastname";
			model.Firstname = "new Firstname";
			model.ExistingEmail = "email@localhost";
			model.NewEmail = "newemail@localhost";
			model.NewUsername = "new username";

			// Act
			RedirectToRouteResult result = _controller.EditUser(model) as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));

			User savedUser = _userService.Users.First();
			Assert.That(savedUser.Lastname, Is.EqualTo(model.Lastname));
			Assert.That(savedUser.Firstname, Is.EqualTo(model.Firstname));
			Assert.That(savedUser.Email, Is.EqualTo(model.NewEmail));
			Assert.That(savedUser.Username, Is.EqualTo(model.NewUsername));
		}

		[Test]
		public void EditUser_POST_Should_Update_Password_When_Password_Is_Not_Empty_And_Redirect_To_Index()
		{
			// Arrange
			User user = new User()
			{
				Id = Guid.NewGuid(),
				IsActivated = true,
				Lastname = "Lastname",
				Firstname = "Firstname",
				Email = "email@localhost",
				Username = "username"
			};
			_userService.Users.Add(user);

			UserViewModel model = new UserViewModel(user);
			model.Password = "NewPassword";
			model.PasswordConfirmation = "NewPassword";

			// Act
			RedirectToRouteResult result = _controller.EditUser(model) as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));

			bool passwordChanged = _userService.Authenticate(user.Email, "NewPassword");
			Assert.That(passwordChanged, Is.True);
		}

		[Test]
		public void EditUser_POST_Should_Return_ViewResult_When_ModelState_Is_Invalid()
		{
			// Arrange
			UserViewModel model = new UserViewModel();
			_controller.ModelState.AddModelError("username", "notvalid"); // force an error

			// Act
			ViewResult result = _controller.EditUser(model) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			UserViewModel resultModel = result.ModelFromActionResult<UserViewModel>();
			Assert.That(resultModel, Is.Not.Null, "resultModel");
		}

		[Test]
		public void Index_Should_Return_View_And_ViewModel_With_Both_User_Types()
		{
			// Arrange
			User admin = new User() { Id = Guid.NewGuid() };
			_userService.Users.Add(admin);

			User editor = new User() { Id = Guid.NewGuid() };
			_userService.Users.Add(editor);

			// Act
			ViewResult result = _controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
			List<IEnumerable<UserViewModel>> model = result.ModelFromActionResult<List<IEnumerable<UserViewModel>>>();
			Assert.That(model, Is.Not.Null, "model");
			Assert.That(model.Count, Is.EqualTo(2));
		}
	}
}
