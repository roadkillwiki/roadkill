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
		private ApplicationSettings _settings;
		private IRepository _repository;
		private Mock<UserServiceBase> _userServiceMock;
		private IUserContext _context;

		[SetUp]
		public void TestsSetup()
		{
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_repository = null;
			_userServiceMock = new Mock<UserServiceBase>(_settings, _repository);
			_userServiceMock.Setup(u => u.UserExists("emailexists@test.com")).Returns(true);
		}

		[Test]
		public void VerifyPassword_When_Created_In_Admin_Tools_With_Bad_Password_Fails()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.Password = "1";
			model.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserViewModel.VerifyPassword(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPassword_For_New_User_With_Bad_Password_Fails()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.Password = "1";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyPassword(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPassword_For_Existing_User_With_Bad_Password_Fails()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = Guid.NewGuid();
			model.Password = "1";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyPassword(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPassword_For_Existing_User_With_Empty_Password_Succeeds()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = Guid.NewGuid();
			model.Password = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyPassword(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPasswordsMatch_When_Created_In_Admin_Tools_With_Mismatching_Passwords_Fails()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.PasswordConfirmation = "1";
			model.Password = "2";
			model.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserViewModel.VerifyPasswordsMatch(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPasswordsMatch_For_Existing_User_With_Matching_Passwords_Succeeds()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.PasswordConfirmation = "password";
			model.Password = "password";
			model.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserViewModel.VerifyPasswordsMatch(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPasswordsMatch_For_New_User_With_Mismatching_Passwords_Fails()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.PasswordConfirmation = "password1";
			model.Password = "password2";
			model.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserViewModel.VerifyPasswordsMatch(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPasswordsMatch_For_Existing_User_With_Empty_Password_Succeeds()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = Guid.NewGuid();
			model.PasswordConfirmation = "";
			model.Password = "";
			model.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserViewModel.VerifyPasswordsMatch(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
