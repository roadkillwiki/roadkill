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
	public class UserViewModelPasswordValidationTests
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

			_userService.Users.Add(new User() { Email = "emailexists@test.com" });
			_userViewModel = new UserViewModel(_applicationSettings, _userService);
		}

		[Test]
		public void verifypassword_when_created_in_admin_tools_with_bad_password_fails()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.Password = "1";

			// Act
			ValidationResult result = UserViewModel.VerifyPassword(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifypassword_for_new_user_with_bad_password_fails()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.Password = "1";

			// Act
			ValidationResult result = UserViewModel.VerifyPassword(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifypassword_for_existing_user_with_bad_password_fails()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.Password = "1";

			// Act
			ValidationResult result = UserViewModel.VerifyPassword(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifypassword_for_existing_user_with_empty_password_succeeds()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.Password = "";

			// Act
			ValidationResult result = UserViewModel.VerifyPassword(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifypasswordsmatch_when_created_in_admin_tools_with_mismatching_passwords_fails()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.PasswordConfirmation = "1";
			_userViewModel.Password = "2";

			// Act
			ValidationResult result = UserViewModel.VerifyPasswordsMatch(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifypasswordsmatch_for_existing_user_with_matching_passwords_succeeds()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.PasswordConfirmation = "password";
			_userViewModel.Password = "password";

			// Act
			ValidationResult result = UserViewModel.VerifyPasswordsMatch(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifypasswordsmatch_for_new_user_with_mismatching_passwords_fails()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.PasswordConfirmation = "password1";
			_userViewModel.Password = "password2";

			// Act
			ValidationResult result = UserViewModel.VerifyPasswordsMatch(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void verifypasswordsmatch_for_existing_user_with_empty_password_succeeds()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.PasswordConfirmation = "";
			_userViewModel.Password = "";

			// Act
			ValidationResult result = UserViewModel.VerifyPasswordsMatch(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
