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
		public void VerifyNewEmail_For_New_User_With_Empty_Email_Should_Fail()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.NewEmail = "";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmail(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_New_User_With_Valid_Email_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.NewEmail = "test@test.com";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmail(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_Existing_User_With_Empty_Email_Should_Fail()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = Guid.NewGuid();
			model.NewEmail = "";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmail(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_Existing_User_With_Valid_Email_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = Guid.NewGuid();
			model.NewEmail = "newemail@test.com";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmail(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_New_User_With_Email_That_Exists_Should_Fail()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.NewEmail = "emailexists@test.com";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_New_User_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = null;
			model.NewEmail = "test@test.com";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_When_New_User_Created_In_Admin_Tools_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.NewEmail = "test@test.com";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Email_That_Exists_Should_Fail()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = Guid.NewGuid();
			model.NewEmail = "emailexists@test.com";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = Guid.NewGuid();
			model.NewEmail = "newemail@test.com";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unchanged_Email_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userServiceMock.Object);
			model.Id = Guid.NewGuid();
			model.ExistingEmail = "newemail@test.com";
			model.NewEmail = "newemail@test.com";
			model.ExistingEmail = "";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewEmailIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
