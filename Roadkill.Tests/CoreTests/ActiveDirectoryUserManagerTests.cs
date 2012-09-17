using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core;
using Roadkill.Tests.Core;
using NUnit.Framework;
using Moq;
using System.DirectoryServices.AccountManagement;
using Moq.Language.Flow;

namespace Roadkill.Tests.Core
{
	/// <summary>
	/// Tests the ActiveDirectory User manager class using stubs for the service.
	/// </summary>
	/// This class promises very little for the actual implementation, as it's untestable without an AD server.
	[TestFixture]
	public class ActiveDirectoryUserManagerTests
	{
		Mock<IActiveDirectoryService> _serviceMock;
		private static readonly string _domain = "domain.com";
		private static readonly string _adminsGroupName = "admins";
		private static readonly string _editorsGroupName = "editors";

		private class MockPrincipal : IRoadKillPrincipal
		{
			public string SamAccountName { get; set; }
		}

		[SetUp]
		public void Setup()
		{
			List<IRoadKillPrincipal> adminUsers = new List<IRoadKillPrincipal>();
			adminUsers.Add(new MockPrincipal() { SamAccountName = "admin1" });
			adminUsers.Add(new MockPrincipal() { SamAccountName = "admin2" });

			List<IRoadKillPrincipal> editorUsers = new List<IRoadKillPrincipal>();
			editorUsers.Add(new MockPrincipal() { SamAccountName = "editor1" });
			editorUsers.Add(new MockPrincipal() { SamAccountName = "editor2" });

			_serviceMock = new Mock<IActiveDirectoryService>();
			_serviceMock.Setup(x => x.GetMembers(_domain, "username", "password", _adminsGroupName)).Returns(adminUsers);
			_serviceMock.Setup(x => x.GetMembers(_domain, "username", "password", _editorsGroupName)).Returns(editorUsers);
		}

		[Test]
		public void Should_Setup_With_Good_Ldap_String()
		{
			// Arrange + Act
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_serviceMock.Object, "LDAP://" + _domain, "username", "password", _editorsGroupName, _adminsGroupName);

			// Assert
			Assert.That(manager, Is.Not.Null);
		}

		[Test]
		public void Admins_Should_Belong_To_Group()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_serviceMock.Object, "LDAP://" + _domain, "username", "password", _editorsGroupName, _adminsGroupName);

			// Act + Assert
			Assert.That(manager.IsAdmin("admin1"), Is.True);
			Assert.That(manager.IsAdmin("admin2"), Is.True);
		}

		[Test]
		public void Editors_Should_Not_Be_Admins()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_serviceMock.Object, "LDAP://" + _domain, "username", "password", _editorsGroupName, _adminsGroupName);

			// Act + Assert
			Assert.That(manager.IsAdmin("editor1"), Is.False);
			Assert.That(manager.IsAdmin("editor2"), Is.False);
		}

		[Test]
		public void Editors_Should_Belong_To_Group()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_serviceMock.Object, "LDAP://" + _domain, "username", "password", _editorsGroupName, _adminsGroupName);

			// Act + Assert
			Assert.That(manager.IsEditor("editor1"), Is.True);
			Assert.That(manager.IsEditor("editor2"), Is.True);
		}
	}
}
