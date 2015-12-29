using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.WebApi
{
	[TestFixture]
	[Category("Unit")]
	public class UserControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private UserServiceMock _userService;
		private IUserContext _userContext;
		private PageRepositoryMock _pageRepositoryMock;
		private UserController _userController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_userContext = _container.UserContext;
			_userService = _container.UserService;
			_pageRepositoryMock = _container.PageRepository;
			_userService = _container.UserService;

			_userController = new UserController(_applicationSettings, _userService, _userContext);
		}

		[Test]
		public void authenticate_should_return_true_user()
		{
			// Arrange
			UserController.UserInfo userInfo = new UserController.UserInfo();
			userInfo.Email = "admin@localhost";
			userInfo.Password = "password1";

			_userService.AddUser("admin@localhost", "user", "password1", true, true);
			_userService.Users[0].IsActivated = true;

			// Act
			bool isAuthed = _userController.Authenticate(userInfo);

			// Assert
			Assert.That(isAuthed, Is.True);
		}

		[Test]
		public void logout_should_user_service_to_logout()
		{
			// Arrange + Act
			_userController.Logout();

			// Assert
			Assert.That(_userService.HasLoggedOut, Is.True);
		}

		[Test]
		public void get_should_return_user()
		{
			// Arrange
			_userService.AddUser("admin@localhost", "user", "password1", true, true);
			User user = _userService.Users[0];
			user.IsActivated = true;

			// Act
			UserViewModel model = _userController.Get(user.Id);

			// Assert
			Assert.That(model.Id, Is.EqualTo(user.Id));
		}

		[Test]
		public void get_should_return_null_when_user_does_not_exist()
		{
			// Arrange

			// Act
			UserViewModel model = _userController.Get(Guid.NewGuid());

			// Assert
			Assert.That(model, Is.Null);
		}

		[Test]
		public void get_should_return_inactive_user()
		{
			// Arrange
			_userService.AddUser("admin@localhost", "user", "password1", false, true);
			User user = _userService.Users[0];

			// Act
			UserViewModel model = _userController.Get(user.Id);

			// Assert
			Assert.That(model.Id, Is.EqualTo(user.Id));
		}

		[Test]
		public void get_should_return_all_editors_and_admins()
		{
			// Arrange
			_userService.AddUser("admin@localhost", "user", "password1", true, true);
			_userService.AddUser("editor@localhost", "user", "password1", false, true);

			// Act
			IEnumerable<UserViewModel> users = _userController.Get();

			// Assert
			Assert.That(users.Count(), Is.EqualTo(2));
		}
	}
}