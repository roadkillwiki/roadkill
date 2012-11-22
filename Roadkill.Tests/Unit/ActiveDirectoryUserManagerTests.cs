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
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Core
{
	/// <summary>
	/// Tests the ActiveDirectory User manager class using stubs for the service.
	/// </summary>
	/// This class promises very little for the actual implementation, as that's untestable without an AD server.
	[TestFixture]
	[Category("Unit")]
	public class ActiveDirectoryUserManagerTests
	{
		private static readonly string _domainPath = "dc=domain.com";
		private static readonly string _adminsGroupName = "admins";
		private static readonly string _editorsGroupName = "editors";
		private static readonly string _ldapString = "LDAP://" + _domainPath;
		private static readonly string _username = "username";
		private static readonly string _password = "password";

		private Mock<IActiveDirectoryService> _serviceMock;
		private IRepository _repository;
		private IConfigurationContainer _config;
		private PageManager _pageManager;

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
			_serviceMock.Setup(x => x.GetMembers(_domainPath, _username, _password, _adminsGroupName)).Returns(adminUsers);
			_serviceMock.Setup(x => x.GetMembers(_domainPath, _username, _password, _editorsGroupName)).Returns(editorUsers);

			_config = new RoadkillSettings();
			_config.SitePreferences = new SitePreferences();
			_config.ApplicationSettings = new ApplicationSettings();
			_config.ApplicationSettings.LdapConnectionString = _ldapString;
			_config.ApplicationSettings.LdapUsername = _username;
			_config.ApplicationSettings.LdapPassword = _password;
			_config.ApplicationSettings.AdminRoleName = _adminsGroupName;
			_config.ApplicationSettings.EditorRoleName = _editorsGroupName;
			_repository = new Mock<IRepository>().Object;
			_pageManager = null; // This can be null for ActiveDirectoryUserManager, but needs to be filled in future.
		}

		[Test]
		public void Should_Setup_With_Good_Ldap_String()
		{
			// Arrange + Act
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);

			// Assert
			Assert.That(manager, Is.Not.Null);
		}

		[Test]
		public void Admins_Should_Belong_To_Group()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);

			// Act + Assert
			Assert.That(manager.IsAdmin("admin1"), Is.True);
			Assert.That(manager.IsAdmin("admin2"), Is.True);
		}

		[Test]
		public void Editors_Should_Not_Be_Admins()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);

			// Act + Assert
			Assert.That(manager.IsAdmin("editor1"), Is.False);
			Assert.That(manager.IsAdmin("editor2"), Is.False);
		}

		[Test]
		public void Editors_Should_Belong_To_Group()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);

			// Act + Assert
			Assert.That(manager.IsEditor("editor1"), Is.True);
			Assert.That(manager.IsEditor("editor2"), Is.True);
		}

		[Test]
		public void GetUser_Should_Return_Object_With_Permissions()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);

			// Act
			User user = manager.GetUser("editor1");

			// Assert
			Assert.That(user, Is.Not.Null);
			Assert.That(user.Email, Is.EqualTo("editor1"));
			Assert.That(user.Username, Is.EqualTo("editor1"));
			Assert.That(user.IsActivated, Is.True);
			Assert.That(user.IsEditor, Is.True);
			Assert.That(user.IsAdmin, Is.False);
		}

		[Test]
		public void ListAdmins_Should_Contain_Correct_Users()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);

			// Act
			List<UserSummary> users = manager.ListAdmins().ToList();

			// Assert
			Assert.That(users.Count, Is.EqualTo(2));
			Assert.That(users.FirstOrDefault(u => u.ExistingUsername == "admin1"), Is.Not.Null);
			Assert.That(users.FirstOrDefault(u => u.ExistingUsername == "admin2"), Is.Not.Null);
		}

		[Test]
		public void ListEditor_Should_Contain_Correct_Users()
		{
			// Arrange
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);

			// Act
			List<UserSummary> users = manager.ListEditors().ToList();

			// Assert
			Assert.That(users.Count, Is.EqualTo(2));
			Assert.That(users.FirstOrDefault(u => u.ExistingUsername == "editor1"), Is.Not.Null);
			Assert.That(users.FirstOrDefault(u => u.ExistingUsername == "editor2"), Is.Not.Null);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void Empty_Ldap_String_Throws_Exception()
		{
			// Arrange + act + assert
			_config.ApplicationSettings.LdapConnectionString = "";
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void Wrong_Format_Ldap_String_Throws_Exception()
		{
			// Arrange + act + assert
			_config.ApplicationSettings.LdapConnectionString = "iforgot.the.ldap.part.com";
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void No_Admin_Group_Throws_Exception()
		{
			// Arrange + act + assert
			_config.ApplicationSettings.AdminRoleName = "";
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void No_Editor_Group_Throws_Exception()
		{
			// Arrange + act + assert
			_config.ApplicationSettings.EditorRoleName = "";
			ActiveDirectoryUserManager manager = new ActiveDirectoryUserManager(_config, _repository, _serviceMock.Object);
		}
	}
}
