using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Import;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.Controllers.SiteSettings
{
	[TestFixture]
	[Category("Unit")]
	public class UserManagementControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private PageRepositoryMock _pageRepository;
		private UserRepositoryMock _userRepository;

		private UserServiceMock _userService;
		private SettingsService _settingsService;

		private UserManagementController _controller;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;

			_pageRepository = _container.PageRepository;
			_userRepository = _container.UserRepository;

			_settingsService = _container.SettingsService;
			_userService = _container.UserService;

			_controller = new UserManagementController(_applicationSettings, _userService, _settingsService, _context);
		}

		[Test]
		public void addadmin_get_should_return_view_and_viewmodel()
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
		public void addadmin_post_should_add_admin_and_redirect_to_index()
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
		public void addadmin_post_should_return_viewresult_when_modelstate_is_invalid()
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
		public void addeditor_get_should_return_view_and_viewmodel()
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
		public void addeditor_post_should_add_admin_and_redirect_to_index()
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
		public void addeditor_post_should_return_viewresult_when_modelstate_is_invalid()
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
		public void deleteuser_should_remove_user_and_redirect_to_index()
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
		public void edituser_get_should_return_view_and_viewmodel()
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
		public void edituser_get_should_redirect_when_user_does_not_exist()
		{
			// Arrange

			// Act
			RedirectToRouteResult result = _controller.EditUser(Guid.NewGuid()) as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
		}

		[Test]
		public void edituser_post_should_update_user_when_username_and_email_changes_and_redirect_to_index()
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
		public void edituser_post_should_update_password_when_password_is_not_empty_and_redirect_to_index()
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
		public void edituser_post_should_return_viewresult_when_modelstate_is_invalid()
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
		public void index_should_return_view_and_viewmodel_with_both_user_types()
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
