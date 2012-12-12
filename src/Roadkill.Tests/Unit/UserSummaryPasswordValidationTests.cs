using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class UserSummaryPasswordValidationTests
	{
		private IConfigurationContainer _config;
		private IRepository _repository;
		private Mock<UserManager> _userManagerMock;
		private IRoadkillContext _context;

		[SetUp]
		public void TestsSetup()
		{
			_context = new Mock<IRoadkillContext>().Object;
			_config = new RoadkillSettings();
			_repository = null;
			_userManagerMock = new Mock<UserManager>(_config, _repository);
			_userManagerMock.Setup(u => u.UserExists("emailexists@test.com")).Returns(true);
		}

		[Test]
		public void VerifyPassword_When_Created_In_Admin_Tools_With_Bad_Password_Fails()
		{
			// Arrange
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
