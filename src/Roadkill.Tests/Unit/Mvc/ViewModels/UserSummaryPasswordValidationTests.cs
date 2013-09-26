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
	public class UserSummaryPasswordValidationTests
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
		public void VerifyPassword_When_Created_In_Admin_Tools_With_Bad_Password_Fails()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.Password = "1";
			summary.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserSummary.VerifyPassword(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPassword_For_New_User_With_Bad_Password_Fails()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.Password = "1";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyPassword(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPassword_For_Existing_User_With_Bad_Password_Fails()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.Password = "1";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyPassword(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPassword_For_Existing_User_With_Empty_Password_Succeeds()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.Password = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyPassword(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPasswordsMatch_When_Created_In_Admin_Tools_With_Mismatching_Passwords_Fails()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.PasswordConfirmation = "1";
			summary.Password = "2";
			summary.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserSummary.VerifyPasswordsMatch(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPasswordsMatch_For_Existing_User_With_Matching_Passwords_Succeeds()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.PasswordConfirmation = "password";
			summary.Password = "password";
			summary.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserSummary.VerifyPasswordsMatch(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPasswordsMatch_For_New_User_With_Mismatching_Passwords_Fails()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = null;
			summary.PasswordConfirmation = "password1";
			summary.Password = "password2";
			summary.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserSummary.VerifyPasswordsMatch(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyPasswordsMatch_For_Existing_User_With_Empty_Password_Succeeds()
		{
			// Arrange
			UserSummary summary = new UserSummary(_settings, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.PasswordConfirmation = "";
			summary.Password = "";
			summary.IsBeingCreatedByAdmin = true;

			// Act
			ValidationResult result = UserSummary.VerifyPasswordsMatch(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}
	}
}
