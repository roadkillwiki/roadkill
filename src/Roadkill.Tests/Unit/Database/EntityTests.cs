using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit.Database
{
	[TestFixture]
	public class EntityTests
	{
		[Test]
		public void User_ObjectId_Should_Match_Id()
		{
			// Arrange
			User user = new User();
			user.Id = Guid.NewGuid();

			// Act
			Guid objectId = user.ObjectId;

			// Assert
			Assert.That(objectId, Is.EqualTo(user.Id));
		}

		[Test]
		public void PageContent_ObjectId_Should_Match_Id()
		{
			// Arrange
			PageContent page = new PageContent();
			page.ObjectId = Guid.NewGuid();

			// Act
			Guid objectId = page.ObjectId;

			// Assert
			Assert.That(objectId, Is.EqualTo(page.Id));
		}

		[Test]
		public void SiteConfigurationEntity_ObjectId_Should_Match_Id()
		{
			// Arrange
			SiteConfigurationEntity siteConfigEntity = new SiteConfigurationEntity();
			siteConfigEntity.ObjectId = Guid.NewGuid();

			// Act
			Guid objectId = siteConfigEntity.ObjectId;

			// Assert
			Assert.That(objectId, Is.EqualTo(siteConfigEntity.Id));
		}
	}
}
