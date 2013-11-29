using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	[TestFixture]
	[Category("Unit")]
	public class UserViewModelTests
	{
		[Test]
		public void Empty_Constructor_Should_Fill_Properties_With_Default_Values()
		{
			// Arrange + Act
			UserViewModel model = new UserViewModel();

			// Assert
			Assert.That(model.Id, Is.Null);
			Assert.That(model.ActivationKey, Is.Null.Or.Empty);
			Assert.That(model.Firstname, Is.Null.Or.Empty);
			Assert.That(model.Lastname, Is.Null.Or.Empty);
			Assert.That(model.ExistingUsername, Is.Null.Or.Empty);
			Assert.That(model.NewUsername, Is.Null.Or.Empty);
			Assert.That(model.ExistingEmail, Is.Null.Or.Empty);
			Assert.That(model.NewEmail, Is.Null.Or.Empty);
			Assert.That(model.Password, Is.Null.Or.Empty);
			Assert.That(model.PasswordConfirmation, Is.Null.Or.Empty);
			Assert.That(model.PasswordResetKey, Is.Null.Or.Empty);
			Assert.That(model.UsernameHasChanged, Is.False);
			Assert.That(model.EmailHasChanged, Is.False);
		}

		[Test]
		public void Constructor_Should_Fill_Properties_From_User_Object()
		{
			// Arrange + Act
			User user = new User();
			user.ActivationKey = Guid.NewGuid().ToString();
			user.Id = Guid.NewGuid();
			user.Email = "user@email.com";
			user.Username = "existingusername";
			user.Firstname = "firstname";
			user.Lastname = user.Lastname;
			user.PasswordResetKey = user.PasswordResetKey;

			UserViewModel model = new UserViewModel(user);

			// Assert
			Assert.That(model.Id, Is.EqualTo(user.Id));
			Assert.That(model.Firstname, Is.EqualTo(user.Firstname));
			Assert.That(model.Lastname, Is.EqualTo(user.Lastname));
			Assert.That(model.ExistingUsername, Is.EqualTo(user.Username));
			Assert.That(model.NewUsername, Is.EqualTo(user.Username));
			Assert.That(model.ExistingEmail, Is.EqualTo(user.Email));
			Assert.That(model.NewEmail, Is.EqualTo(user.Email));
			Assert.That(model.PasswordResetKey, Is.EqualTo(user.PasswordResetKey));
			Assert.That(model.UsernameHasChanged, Is.False);
			Assert.That(model.EmailHasChanged, Is.False);
		}

		[Test]
		public void Email_And_Username_Changed_Should_Be_True_When_Properties_Changed()
		{
			// Arrange + Act
			UserViewModel model = new UserViewModel();
			model.ExistingUsername = "previous-username";
			model.NewUsername = "new-username";
			model.ExistingEmail = "oldemail@email.com";
			model.NewEmail = "newemail@email.com";

			// Assert
			Assert.That(model.UsernameHasChanged, Is.True);
			Assert.That(model.EmailHasChanged, Is.True);
		}
	}
}
