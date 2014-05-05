using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Permissions;
using Roadkill.Core.Security.Permissions.Capabilities;

namespace Roadkill.Tests.Unit.Security
{
	[TestFixture]
	[Category("Unit")]
	public class RoleTests
	{
		[Test]
		[Explicit("Used for creating the Roles.config file - not a real test.")]
		public void Serialize()
		{
			// Arrange
			List<Role> rolesList = new List<Role>();
			rolesList.Add(Role.SuperAdmin);
			rolesList.Add(Role.Admin);

			// Act
			string xml = ConfigFileSerializer.Serialize(rolesList);

			// Assert
			Console.WriteLine(xml);
		}

		[Test]
		public void Constructor_Should_Create_Empty_Capability_Lists()
		{
			// Arrange and Act
			Role role = new Role();

			// Assert
			Assert.That(role.IOCapabilities, Is.Not.Null);
			Assert.That(role.PageCapabilities, Is.Not.Null);
			Assert.That(role.SiteSettingsCapabilities, Is.Not.Null);
		}

		[Test]
		public void SuperAdmin_Should_Have_All_Capabilities()
		{
			// Arrange 
			var allIO = Enum.GetValues(typeof(IOCapability));
			var allPages = Enum.GetValues(typeof(PageCapability));
			var allSettings = Enum.GetValues(typeof(SiteSettingsCapability));
			
			// Act
			Role superAdmin = Role.SuperAdmin;

			// Assert
			Assert.That(superAdmin.IOCapabilities, Is.EquivalentTo(allIO));
			Assert.That(superAdmin.PageCapabilities, Is.EquivalentTo(allPages));
			Assert.That(superAdmin.SiteSettingsCapabilities, Is.EquivalentTo(allSettings));
		}

		[Test]
		public void Admin_Should_Have_All_Capabilities()
		{
			// Arrange 
			var allIO = Enum.GetValues(typeof(IOCapability));
			var allPages = Enum.GetValues(typeof(PageCapability));
			var allSettings = Enum.GetValues(typeof(SiteSettingsCapability));
			
			// Act
			Role superAdmin = Role.Admin;

			// Assert
			Assert.That(superAdmin.IOCapabilities, Is.EquivalentTo(allIO));
			Assert.That(superAdmin.PageCapabilities, Is.EquivalentTo(allPages));
			Assert.That(superAdmin.SiteSettingsCapabilities, Is.EquivalentTo(allSettings));
		}
	}
}
