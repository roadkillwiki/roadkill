using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class FromEntityTests
	{
		[Test]
		public void ToPage_ShouldFillCorrectProperties()
		{
			// Arrange
			PageEntity entity = new PageEntity();
			entity.CreatedBy = "createdtest";
			entity.CreatedOn = DateTime.UtcNow;
			entity.IsLocked = true;
			entity.ModifiedBy = "modtest";
			entity.ModifiedOn = DateTime.UtcNow.AddYears(1);
			entity.Tags = "tag1,tag2";
			entity.Title = "title1";
			
			// Act
			Page page = FromEntity.ToPage(entity);

			// Assert
			Assert.That(page.CreatedBy, Is.EqualTo(entity.CreatedBy));
			Assert.That(page.CreatedOn, Is.EqualTo(entity.CreatedOn));
			Assert.That(page.IsLocked, Is.EqualTo(entity.IsLocked));
			Assert.That(page.ModifiedBy, Is.EqualTo(entity.ModifiedBy));
			Assert.That(page.ModifiedOn, Is.EqualTo(entity.ModifiedOn));
			Assert.That(page.Tags, Is.EqualTo(entity.Tags));
			Assert.That(page.Title, Is.EqualTo(entity.Title));
		}

		[Test]
		public void ToPageContent_ShouldFillCorrectProperties()
		{
			// Arrange
			PageContentEntity entity = new PageContentEntity();
			entity.Page = new PageEntity() { Title = "Page title" };
			//entity.Id = Guid.NewGuid(); // can't be tested
			entity.EditedBy = "editedtest";
			entity.EditedOn = DateTime.UtcNow;
			entity.Text = "text";
			entity.VersionNumber = 99;

			// Act
			PageContent pageContent = FromEntity.ToPageContent(entity);

			// Assert
			Assert.That(pageContent.EditedBy, Is.EqualTo(entity.EditedBy));
			Assert.That(pageContent.EditedOn, Is.EqualTo(entity.EditedOn));
			Assert.That(pageContent.Text, Is.EqualTo(entity.Text));
			Assert.That(pageContent.VersionNumber, Is.EqualTo(pageContent.VersionNumber));
			Assert.That(pageContent.Page, Is.Not.Null);
			Assert.That(pageContent.Page.Title, Is.EqualTo(pageContent.Page.Title));
		}

		[Test]
		public void ToUser_ShouldFillCorrectProperties()
		{
			// Arrange
			UserEntity entity = new UserEntity();
			//entity.Id = Guid.NewGuid(); // can't be tested
			entity.ActivationKey = "key";
			entity.Email = "email";
			entity.Firstname = "firstname";
			entity.IsActivated = true;
			entity.IsAdmin = true;
			entity.IsEditor = true;
			entity.Lastname = "lastname";
			entity.PasswordResetKey = "resetkey";
			entity.Password = "password";
			entity.Salt = "salt";

			// Act
			User user = FromEntity.ToUser(entity);

			// Assert
			Assert.That(user.Id, Is.EqualTo(entity.Id));
			Assert.That(user.ActivationKey, Is.EqualTo(entity.ActivationKey));
			Assert.That(user.Email, Is.EqualTo(entity.Email));
			Assert.That(user.Firstname, Is.EqualTo(entity.Firstname));
			Assert.That(user.IsActivated, Is.EqualTo(entity.IsActivated));
			Assert.That(user.IsAdmin, Is.EqualTo(entity.IsAdmin));
			Assert.That(user.IsEditor, Is.EqualTo(entity.IsEditor));
			Assert.That(user.Lastname, Is.EqualTo(entity.Lastname));
			Assert.That(user.PasswordResetKey, Is.EqualTo(entity.PasswordResetKey));
			Assert.That(user.Password, Is.EqualTo(entity.Password));
			Assert.That(user.Salt, Is.EqualTo(entity.Salt));
		}

		[Test]
		public void ToPageContentList_ShouldContainValidList()
		{
			// Arrange
			PageContentEntity entity1 = new PageContentEntity();
			entity1.EditedBy = "editedtest1";
			entity1.EditedOn = DateTime.UtcNow;
			entity1.Text = "text";
			entity1.VersionNumber = 90;		

			PageContentEntity entity2 = new PageContentEntity();
			entity2.EditedBy = "editedtest2";
			entity2.EditedOn = DateTime.UtcNow;
			entity2.Text = "text";
			entity2.VersionNumber = 91;		

			PageContentEntity entity3 = new PageContentEntity();
			entity3.EditedBy = "editedtest3";
			entity3.EditedOn = DateTime.UtcNow;
			entity3.Text = "text";
			entity3.VersionNumber = 92;

			List<PageContentEntity> entityList = new List<PageContentEntity>();
			entityList.Add(entity1);
			entityList.Add(entity2);
			entityList.Add(entity3);

			// Act
			List<PageContent> contentList = FromEntity.ToPageContentList(entityList).ToList();

			// Assert
			Assert.That(contentList.Count, Is.EqualTo(3));
			Assert.That(contentList[0].VersionNumber, Is.EqualTo(entity1.VersionNumber));
			Assert.That(contentList[1].VersionNumber, Is.EqualTo(entity2.VersionNumber));
			Assert.That(contentList[2].VersionNumber, Is.EqualTo(entity3.VersionNumber));
		}

		[Test]
		public void ToPageList_ShouldContainValidList()
		{
			// Arrange
			PageEntity entity1 = new PageEntity();
			entity1.CreatedBy = "createdtest1";
			entity1.CreatedOn = DateTime.UtcNow;
			entity1.IsLocked = true;
			entity1.ModifiedBy = "modtest1";
			entity1.ModifiedOn = DateTime.UtcNow.AddYears(1);
			entity1.Tags = "tag1,tag2";
			entity1.Title = "title1";

			PageEntity entity2 = new PageEntity();
			entity2.CreatedBy = "createdtest2";
			entity2.CreatedOn = DateTime.UtcNow;
			entity2.IsLocked = true;
			entity2.ModifiedBy = "modtest2";
			entity2.ModifiedOn = DateTime.UtcNow.AddYears(2);
			entity2.Tags = "tag2";
			entity2.Title = "title2";

			PageEntity entity3 = new PageEntity();
			entity3.CreatedBy = "createdtest3";
			entity3.CreatedOn = DateTime.UtcNow;
			entity3.IsLocked = true;
			entity3.ModifiedBy = "modtest3";
			entity3.ModifiedOn = DateTime.UtcNow.AddYears(3);
			entity3.Tags = "tagtag3";
			entity3.Title = "title3";

			List<PageEntity> entities = new List<PageEntity>();
			entities.Add(entity1);
			entities.Add(entity2);
			entities.Add(entity3);

			// Act
			List<Page> pageList = FromEntity.ToPageList(entities).ToList();

			// Assert
			Assert.That(pageList.Count, Is.EqualTo(3));
			Assert.That(pageList[0].Title, Is.EqualTo(entity1.Title));
			Assert.That(pageList[1].Title, Is.EqualTo(entity2.Title));
			Assert.That(pageList[2].Title, Is.EqualTo(entity3.Title));
		}

		[Test]
		public void ToUserList_ShouldContainValidList()
		{
			// Arrange
			UserEntity entity1  = new UserEntity();
			entity1.ActivationKey = "key1";
			entity1.Email = "email1";
			entity1.Firstname = "firstname1";
			entity1.IsActivated = true;
			entity1.IsAdmin = true;
			entity1.IsEditor = true;
			entity1.Lastname = "lastname1";
			entity1.Password = "pwd1";
			entity1.PasswordResetKey = "resetkey1";
			entity1.Salt = "salt1";

			UserEntity entity2 = new UserEntity();
			entity2.ActivationKey = "key2";
			entity2.Email = "email1";
			entity2.Firstname = "firstname1";
			entity2.IsActivated = true;
			entity2.IsAdmin = true;
			entity2.IsEditor = true;
			entity2.Lastname = "lastname2";
			entity2.Password = "pwd2";
			entity2.PasswordResetKey = "resetkey2";
			entity2.Salt = "salt2";

			UserEntity entity3 = new UserEntity();
			entity3.ActivationKey = "key3";
			entity3.Email = "email3";
			entity3.Firstname = "firstname3";
			entity3.IsActivated = true;
			entity3.IsAdmin = true;
			entity3.IsEditor = true;
			entity3.Lastname = "lastname3";
			entity3.Password = "pwd3";
			entity3.PasswordResetKey = "resetkey3";
			entity3.Salt = "salt3";

			List<UserEntity> entities = new List<UserEntity>();
			entities.Add(entity1);
			entities.Add(entity2);
			entities.Add(entity3);

			// Act
			List<User> userList = FromEntity.ToUserList(entities).ToList();

			// Assert
			Assert.That(userList.Count, Is.EqualTo(3));
			Assert.That(userList[0].Salt, Is.EqualTo(entity1.Salt));
			Assert.That(userList[1].Salt, Is.EqualTo(entity2.Salt));
			Assert.That(userList[2].Salt, Is.EqualTo(entity3.Salt));
		}

		// Should return nulls
	}
}