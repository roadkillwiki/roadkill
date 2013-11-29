using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class UserViewModelUsernameValidationTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private IUserContext _context;

		private UserViewModel _userViewModel;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_userService = _container.UserService;

			_userService.Users.Add(new User() { Username = "username-exists" });
			_userViewModel = new UserViewModel(_applicationSettings, _userService);
		}

		[Test]
		public void VerifyNewUsername_For_New_User_With_Blank_Username_Should_Fail()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.NewUsername = "			\n";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsername(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_New_User_With_Valid_Username_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.NewUsername = "fred1234";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsername(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_Existing_User_With_Blank_Username_Should_Fail()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.ExistingUsername = "hansblix";
			_userViewModel.NewUsername = "			\n";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsername(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_Existing_User_With_Valid_Username_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.ExistingUsername = "hansblix";
			_userViewModel.NewUsername = "fred1234";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsername(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_New_User_With_Username_That_Exists_Should_Fail()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.ExistingUsername = "hansblix";
			_userViewModel.NewUsername = "username-exists";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_New_User_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.ExistingUsername = "hansblix";
			_userViewModel.NewUsername = "a_unique_name";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_When_New_User_Created_In_Admin_Tools_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.ExistingUsername = "hansblix";
			_userViewModel.NewUsername = "a_unique_name";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_Existing_User_With_username_That_Exists_Should_Fail()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.ExistingUsername = "hansblix";
			_userViewModel.NewUsername = "username-exists";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_Existing_User_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.ExistingUsername = "hansblix";
			_userViewModel.NewUsername = "hansblix2";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unchanged_Username_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.ExistingUsername = "hansblix";
			_userViewModel.NewUsername = "hansblix";

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
