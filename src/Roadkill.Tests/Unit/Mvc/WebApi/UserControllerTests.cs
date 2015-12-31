using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebApi;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.WebApi
{
	[TestFixture]
	[Category("Unit")]
	public class UserControllerTests
	{
		private MocksAndStubsContainer _container;

		private UserServiceMock _userService;
		private UserController _userController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_userService = _container.UserService;
			_userController = new UserController(_userService);
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