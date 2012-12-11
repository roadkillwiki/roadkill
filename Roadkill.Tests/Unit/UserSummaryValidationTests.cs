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
	public class UserSummaryValidationTests
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
		public void VerifyNewEmail_For_New_User_With_No_Email_Is_Invalid()
		{
			// Arrange
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
		public void VerifyNewEmailIsNotInUse_For_New_User_With_Email_Is_Valid()
		{
			// Arrange
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
		public void VerifyNewEmailIsNotInUse_For_New_User_With_Already_Existing_Email_Is_Invalid()
		{
			// Arrange
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
		public void VerifyNewEmail_For_Existing_User_With_No_Email_Is_Invalid()
		{
			// Arrange
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
		public void VerifyNewEmail_For_Existing_User_With_New_Email_Is_Valid()
		{
			// Arrange
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
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
		public void VerifyNewEmailIsNotInUse_For_Existing_User_With_Already_Existing_Email_Is_Invalid()
		{
			// Arrange
			UserSummary summary = new UserSummary(_config, _userManagerMock.Object);
			summary.Id = Guid.NewGuid();
			summary.NewEmail = "emailexists@test.com";
			summary.ExistingEmail = "";
			summary.IsBeingCreatedByAdmin = false;

			// Act
			ValidationResult result = UserSummary.VerifyNewEmailIsNotInUse(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}
	}
}
