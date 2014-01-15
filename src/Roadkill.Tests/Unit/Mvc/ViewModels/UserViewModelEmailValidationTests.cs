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
	public class UserViewModelEmailValidationTests
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
		public void VerifyNewEmail_For_New_User_With_Empty_Email_Should_Fail()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.NewEmail = "";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmail(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_New_User_With_Valid_Email_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.NewEmail = "test@test.com";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmail(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_Existing_User_With_Empty_Email_Should_Fail()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.NewEmail = "";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmail(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_Existing_User_With_Valid_Email_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.NewEmail = "newemail@test.com";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmail(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_New_User_With_Email_That_Exists_Should_Fail()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.NewEmail = "emailexists@test.com";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_New_User_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = null;
			_userViewModel.NewEmail = "test@test.com";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_When_New_User_Created_In_Admin_Tools_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			_userViewModel.NewEmail = "test@test.com";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Email_That_Exists_Should_Fail()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.NewEmail = "emailexists@test.com";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.NewEmail = "newemail@test.com";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unchanged_Email_Should_Succeed()
		{
			// Arrange
			_userViewModel.Id = Guid.NewGuid();
			_userViewModel.ExistingEmail = "newemail@test.com";
			_userViewModel.NewEmail = "newemail@test.com";
			_userViewModel.ExistingEmail = "";

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(_userViewModel, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
