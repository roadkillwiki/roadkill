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
	public class UserSummaryEmailValidationTests
	{
		private ApplicationSettings _settings;
		private IRepository _repository;
		private Mock<UserManagerBase> _userManagerMock;
		private IUserContext _context;

		[SetUp]
		public void TestsSetup()
		{
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_repository = null;
			_userManagerMock = new Mock<UserManagerBase>(_settings, _repository);
			_userManagerMock.Setup(u => u.UserExists("emailexists@test.com")).Returns(true);
		}

		[Test]
		public void VerifyNewEmail_For_New_User_With_Empty_Email_Should_Fail()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.NewEmail = "";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmail(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_New_User_With_Valid_Email_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.NewEmail = "test@test.com";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmail(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_Existing_User_With_Empty_Email_Should_Fail()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.NewEmail = "";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmail(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmail_For_Existing_User_With_Valid_Email_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.NewEmail = "newemail@test.com";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmail(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_New_User_With_Email_That_Exists_Should_Fail()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.NewEmail = "emailexists@test.com";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmailIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_New_User_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.NewEmail = "test@test.com";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmailIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_When_New_User_Created_In_Admin_Tools_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.NewEmail = "test@test.com";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmailIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Email_That_Exists_Should_Fail()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.NewEmail = "emailexists@test.com";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmailIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unique_Email_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.NewEmail = "newemail@test.com";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmailIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Unchanged_Email_Should_Succeed()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.ExistingEmail = "newemail@test.com";
			summary.NewEmail = "newemail@test.com";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmailIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
