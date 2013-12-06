using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Database.LightSpeed;
using Mindscape.LightSpeed;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class ToEntityTests
	{
		[Test]
		public void FromUser_ShouldFillCorrectProperties()
		{
			// Arrange
			User user = new User();
			user.Id = Guid.NewGuid();
			user.ActivationKey = "key";
			user.Email = "email";
			user.Firstname = "firstname";
			user.Id = Guid.NewGuid();
			user.IsActivated = true;
			user.IsAdmin = true;
			user.IsEditor = true;
			user.Lastname = "lastname";
			user.SetPassword("pwd");
			user.PasswordResetKey = "resetkey";
			user.Salt = "salt";

			UserEntity entity = new UserEntity();

			// Act
			ToEntity.FromUser(user, entity);

			// Assert
			Assert.That(entity.Id, Is.Not.EqualTo(user.Id));  // the id isn't copied from the page
			Assert.That(entity.ActivationKey, Is.EqualTo(user.ActivationKey));
			Assert.That(entity.Email, Is.EqualTo(user.Email));
			Assert.That(entity.Firstname, Is.EqualTo(user.Firstname));
			Assert.That(entity.IsActivated, Is.EqualTo(user.IsActivated));
			Assert.That(entity.IsAdmin, Is.EqualTo(user.IsAdmin));
			Assert.That(entity.IsEditor, Is.EqualTo(user.IsEditor));
			Assert.That(entity.Lastname, Is.EqualTo(user.Lastname));
			Assert.That(entity.Password, Is.EqualTo(user.Password));
			Assert.That(entity.Salt, Is.EqualTo(user.Salt));
		}

		[Test]
		public void FromPage_ShouldFillCorrectProperties()
		{
			// Arrange
			Page page = new Page();
			page.Id = 123;
			page.CreatedBy = "createdby";
			page.CreatedOn = DateTime.UtcNow;
			page.IsLocked = true;
			page.ModifiedBy = "modifiedby";
			page.ModifiedOn = DateTime.UtcNow;
			page.Tags = "tag1,tag2";
			page.Title = "title";

			PageEntity entity = new PageEntity();

			// Act
			ToEntity.FromPage(page, entity);

			// Assert
			Assert.That(entity.Id, Is.Not.EqualTo(page.Id)); // the id isn't copied from the page
			Assert.That(entity.CreatedBy, Is.EqualTo(page.CreatedBy));
			Assert.That(entity.CreatedOn, Is.EqualTo(page.CreatedOn));
			Assert.That(entity.IsLocked, Is.EqualTo(page.IsLocked));
			Assert.That(entity.ModifiedBy, Is.EqualTo(page.ModifiedBy));
			Assert.That(entity.ModifiedOn, Is.EqualTo(page.ModifiedOn));
			Assert.That(entity.Tags, Is.EqualTo(page.Tags));
			Assert.That(entity.Title, Is.EqualTo(page.Title));
		}

		[Test]
		public void FromPageContent_ShouldFillCorrectProperties()
		{
			// Arrange
			PageContent pageContent = new PageContent();
			pageContent.Id = Guid.NewGuid();
			pageContent.EditedBy = "editedby";
			pageContent.EditedOn = DateTime.UtcNow;
			pageContent.Text = "text";

			PageContentEntity entity = new PageContentEntity();

			// Act
			ToEntity.FromPageContent(pageContent, entity);

			// Assert
			Assert.That(entity.Id, Is.Not.EqualTo(pageContent.Id));  // the id isn't copied from the page
			Assert.That(entity.EditedBy, Is.EqualTo(pageContent.EditedBy));
			Assert.That(entity.EditedOn, Is.EqualTo(pageContent.EditedOn));
			Assert.That(entity.Text, Is.EqualTo(pageContent.Text));
			Assert.That(entity.Page	, Is.Null);
		}
	}
}
