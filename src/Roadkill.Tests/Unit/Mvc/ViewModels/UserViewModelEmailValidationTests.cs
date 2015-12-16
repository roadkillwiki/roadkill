using System;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
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
		public void verifynewemail_for_new_user_with_empty_email_should_fail()
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
		public void verifynewemail_for_new_user_with_valid_email_should_succeed()
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
		public void verifynewemail_for_existing_user_with_empty_email_should_fail()
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
		public void verifynewemail_for_existing_user_with_valid_email_should_succeed()
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
		public void verifynewemailisnotinuse_for_new_user_with_email_that_exists_should_fail()
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
		public void verifynewemailisnotinuse_for_new_user_with_unique_email_should_succeed()
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
		public void verifynewemailisnotinuse_when_new_user_created_in_admin_tools_with_unique_email_should_succeed()
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
		public void verifynewemailisnotinuse_for_existing_user_with_email_that_exists_should_fail()
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
		public void verifynewemailisnotinuse_for_existing_user_with_unique_email_should_succeed()
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
		public void verifynewemailisnotinuse_for_existing_user_with_unchanged_email_should_succeed()
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
