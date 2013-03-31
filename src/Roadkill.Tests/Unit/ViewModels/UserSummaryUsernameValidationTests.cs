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

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class UserSummaryUsernameValidationTests
	{
		private ApplicationSettings _settings;
		private IRepository _repository;
		private Mock<UserManager> _userManagerMock;
		private IRoadkillContext _context;

		[SetUp]
		public void TestsSetup()
		{
			_context = new Mock<IRoadkillContext>().Object;
			_settings = new ApplicationSettings();
			_repository = null;
			_userManagerMock = new Mock<UserManager>(_settings, _repository);
			_userManagerMock.Setup(u => u.UserNameExists("username-exists")).Returns(true);
		}

		[Test]
		public void VerifyNewUsername_For_New_User_With_Blank_Username_Should_Fail()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.NewUsername = "			\n";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsername(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_New_User_With_Valid_Username_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.NewUsername = "fred1234";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsername(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_Existing_User_With_Blank_Username_Should_Fail()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.ExistingUsername = "hansblix";
			summary.NewUsername = "			\n";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsername(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsername_For_Existing_User_With_Valid_Username_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.ExistingUsername = "hansblix";
			summary.NewUsername = "fred1234";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsername(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_New_User_With_Username_That_Exists_Should_Fail()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.ExistingUsername = "hansblix";
			summary.NewUsername = "username-exists";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsernameIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_New_User_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.ExistingUsername = "hansblix";
			summary.NewUsername = "a_unique_name";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsernameIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_When_New_User_Created_In_Admin_Tools_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.ExistingUsername = "hansblix";
			summary.NewUsername = "a_unique_name";
			summary.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsernameIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_Existing_User_With_username_That_Exists_Should_Fail()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.ExistingUsername = "hansblix";
			summary.NewUsername = "username-exists";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsernameIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewUsernameIsNotInUse_For_Existing_User_With_Unique_Username_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.ExistingUsername = "hansblix";
			summary.NewUsername = "hansblix2";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsernameIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unchanged_Username_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.ExistingUsername = "hansblix";
			summary.NewUsername = "hansblix";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewUsernameIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
