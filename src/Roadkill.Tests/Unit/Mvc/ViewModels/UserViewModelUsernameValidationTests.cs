using System;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
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
		public void verifynewusername_for_new_user_with_blank_username_should_fail()
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
		public void verifynewusername_for_new_user_with_valid_username_should_succeed()
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
		public void verifynewusername_for_existing_user_with_blank_username_should_fail()
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
		public void verifynewusername_for_existing_user_with_valid_username_should_succeed()
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
		public void verifynewusernameisnotinuse_for_new_user_with_username_that_exists_should_fail()
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
		public void verifynewusernameisnotinuse_for_new_user_with_unique_username_should_succeed()
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
		public void verifynewusernameisnotinuse_when_new_user_created_in_admin_tools_with_unique_username_should_succeed()
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
		public void verifynewusernameisnotinuse_for_existing_user_with_username_that_exists_should_fail()
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
		public void verifynewusernameisnotinuse_for_existing_user_with_unique_username_should_succeed()
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
		public void verifynewemailisnotinuse_for_existing_user_with_unchanged_username_should_succeed()
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
