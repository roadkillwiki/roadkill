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
		public void VerifyPassword_When_Created_In_Admin_Tools_With_Bad_Password_Fails()
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
		public void VerifyPassword_For_New_User_With_Bad_Password_Fails()
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
		public void VerifyPassword_For_Existing_User_With_Bad_Password_Fails()
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
		public void VerifyPassword_For_Existing_User_With_Empty_Password_Succeeds()
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
		public void VerifyPasswordsMatch_When_Created_In_Admin_Tools_With_Mismatching_Passwords_Fails()
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
		public void VerifyPasswordsMatch_For_Existing_User_With_Matching_Passwords_Succeeds()
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
		public void VerifyPasswordsMatch_For_New_User_With_Mismatching_Passwords_Fails()
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
		public void VerifyPasswordsMatch_For_Existing_User_With_Empty_Password_Succeeds()
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
