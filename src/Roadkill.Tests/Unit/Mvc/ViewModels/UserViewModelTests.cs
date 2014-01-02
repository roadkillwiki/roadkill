using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Configuration;
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
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_Should_Throw_ArgumentException_When_User_Object_Is_Null()
		{
			// Arrange
			User user = null;

			// Act + Assert
			UserViewModel model = new UserViewModel(user);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_Should_Throw_ArgumentException_When_Settings_Is_Null()
		{
			// Arrange + Act + Assert
			UserViewModel model = new UserViewModel(null, new UserServiceStub());
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Constructor_Should_Throw_ArgumentException_When_UserService_Is_Null()
		{
			// Arrange + Act + Assert
			UserViewModel model = new UserViewModel(new ApplicationSettings(), null);
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

		[Test]
		public void Equals_Should_Compare_By_ExistingEmail()
		{
			// Arrange + Act
			UserViewModel user1 = new UserViewModel();
			user1.ExistingEmail = "user1@email.com";

			UserViewModel user2 = new UserViewModel();
			user2.ExistingEmail = "user2@email.com";

			// Assert
			Assert.False(user1.Equals(user2));
		}

		[Test]
		public void GetHashCode_Should_Compare_By_ExistingEmail()
		{
			// Arrange + Act
			UserViewModel user1 = new UserViewModel();
			user1.ExistingEmail = "user1@email.com";

			UserViewModel user2 = new UserViewModel();
			user2.ExistingEmail = "user2@email.com";

			// Assert
			Assert.That(user1.GetHashCode(), Is.Not.EqualTo(user2.GetHashCode()));
		}

		[Test]
		[Description("A check for the webapi use of unioning two userviewmodel sets")]
		public void Union_Should_Removed_Duplicate_Emails()
		{
			// Arrange + Act
			UserViewModel user1 = new UserViewModel();
			user1.ExistingEmail = "user1@email.com";

			UserViewModel user2 = new UserViewModel();
			user2.ExistingEmail = "user2@email.com";

			UserViewModel user2Again = new UserViewModel();
			user2Again.ExistingEmail = "user2@email.com";

			List<UserViewModel> userList1 = new List<UserViewModel>()
			{
				user1, user2, user2Again
			};

			List<UserViewModel> userList2 = new List<UserViewModel>()
			{
				user1, user2, user2Again
			};

			// Assert
			Assert.That(userList1.Union(userList2).Count(), Is.EqualTo(2));
		}

		[Test]
		[Description("A similar check to the Union test (it may now be redundant)")]
		public void IEquatable_With_Distinct_Should_Removed_Duplicate_Emails()
		{
			// Arrange + Act
			UserViewModel user1 = new UserViewModel();
			user1.ExistingEmail = "user1@email.com";

			UserViewModel user2 = new UserViewModel();
			user2.ExistingEmail = "user2@email.com";

			UserViewModel user2Again = new UserViewModel();
			user2Again.ExistingEmail = "user2@email.com";

			List<UserViewModel> users = new List<UserViewModel>()
			{
				user1, user2, user2Again
			};

			// Assert
			Assert.That(users.Distinct().Count(), Is.EqualTo(2));
		}
	}
}
