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
		private ApplicationSettings _settings;
		private IRepository _repository;
		private Mock<UserServiceBase> _userManagerMock;
		private IUserContext _context;

		[SetUp]
		public void SetUp()
		{
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_repository = null;
			_userManagerMock = new Mock<UserServiceBase>(_settings, _repository);
			_userManagerMock.Setup(u => u.UserNameExists("username-exists")).Returns(true);
		}

		[Test]
		public void VerifyNewUsername_For_New_User_With_Blank_Username_Should_Fail()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = null;
			model.NewUsername = "			\n";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsername(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_New_User_With_Valid_Username_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = null;
			model.NewUsername = "fred1234";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsername(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_Existing_User_With_Blank_Username_Should_Fail()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = Guid.NewGuid();
			model.ExistingUsername = "hansblix";
			model.NewUsername = "			\n";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsername(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_Existing_User_With_Valid_Username_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = Guid.NewGuid();
			model.ExistingUsername = "hansblix";
			model.NewUsername = "fred1234";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsername(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_New_User_With_Username_That_Exists_Should_Fail()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = null;
			model.ExistingUsername = "hansblix";
			model.NewUsername = "username-exists";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_New_User_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = null;
			model.ExistingUsername = "hansblix";
			model.NewUsername = "a_unique_name";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_When_New_User_Created_In_Admin_Tools_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = null;
			model.ExistingUsername = "hansblix";
			model.NewUsername = "a_unique_name";
			model.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_Existing_User_With_username_That_Exists_Should_Fail()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = Guid.NewGuid();
			model.ExistingUsername = "hansblix";
			model.NewUsername = "username-exists";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_Existing_User_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = Guid.NewGuid();
			model.ExistingUsername = "hansblix";
			model.NewUsername = "hansblix2";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unchanged_Username_Should_Succeed()
		{
			// Arrange
			UserViewModel model = new UserViewModel(_settings, _userManagerMock.Object);
			model.Id = Guid.NewGuid();
			model.ExistingUsername = "hansblix";
			model.NewUsername = "hansblix";
			model.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserViewModel.VerifyNewUsernameIsNotInUse(model, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
