#if !MONO
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security.Windows;

namespace Roadkill.Tests.Unit.Services
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

		private MocksAndStubsContainer _container;

		private IUserRepository _repository;
		private ApplicationSettings _applicationSettings;
		private Mock<IActiveDirectoryProvider> _adProviderMock;
		private ActiveDirectoryUserService _userService;

		private class MockPrincipal : IPrincipalDetails
		{
			public string SamAccountName { get; set; }
		}

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.LdapConnectionString = _ldapString;
			_applicationSettings.LdapUsername = _username;
			_applicationSettings.LdapPassword = _password;
			_applicationSettings.AdminRoleName = _adminsGroupName;
			_applicationSettings.EditorRoleName = _editorsGroupName;
			_repository = _container.UserRepository;

			List<IPrincipalDetails> adminUsers = new List<IPrincipalDetails>();
			adminUsers.Add(new MockPrincipal() { SamAccountName = "admin1" });
			adminUsers.Add(new MockPrincipal() { SamAccountName = "admin2" });

			List<IPrincipalDetails> editorUsers = new List<IPrincipalDetails>();
			editorUsers.Add(new MockPrincipal() { SamAccountName = "editor1" });
			editorUsers.Add(new MockPrincipal() { SamAccountName = "editor2" });

			_adProviderMock = new Mock<IActiveDirectoryProvider>();
			_adProviderMock.Setup(x => x.GetMembers(_domainPath, _username, _password, _adminsGroupName)).Returns(adminUsers);
			_adProviderMock.Setup(x => x.GetMembers(_domainPath, _username, _password, _editorsGroupName)).Returns(editorUsers);

			_userService = new ActiveDirectoryUserService(_applicationSettings, _repository, _adProviderMock.Object);
		}

		[Test]
		public void admins_should_belong_to_group()
		{
			// Arrange

			// Act + Assert
			Assert.That(_userService.IsAdmin("admin1"), Is.True);
			Assert.That(_userService.IsAdmin("admin2"), Is.True);
		}

		[Test]
		public void editors_should_not_be_admins()
		{
			// Arrange		

			// Act + Assert
			Assert.That(_userService.IsAdmin("editor1"), Is.False);
			Assert.That(_userService.IsAdmin("editor2"), Is.False);
		}

		[Test]
		public void editors_should_belong_to_group()
		{
			// Arrange
			ActiveDirectoryUserService service = new ActiveDirectoryUserService(_applicationSettings, _repository, _adProviderMock.Object);

			// Act + Assert
			Assert.That(service.IsEditor("editor1"), Is.True);
			Assert.That(service.IsEditor("editor2"), Is.True);
		}

		[Test]
		public void getuser_should_return_object_with_permissions()
		{
			// Arrange			

			// Act
			User user = _userService.GetUser("editor1");

			// Assert
			Assert.That(user, Is.Not.Null);
			Assert.That(user.Email, Is.EqualTo("editor1"));
			Assert.That(user.Username, Is.EqualTo("editor1"));
			Assert.That(user.IsActivated, Is.True);
			Assert.That(user.IsEditor, Is.True);
			Assert.That(user.IsAdmin, Is.False);
		}

		[Test]
		public void listadmins_should_contain_correct_users()
		{
			// Arrange

			// Act
			List<UserViewModel> users = _userService.ListAdmins().ToList();

			// Assert
			Assert.That(users.Count, Is.EqualTo(2));
			Assert.That(users.FirstOrDefault(u => u.ExistingUsername == "admin1"), Is.Not.Null);
			Assert.That(users.FirstOrDefault(u => u.ExistingUsername == "admin2"), Is.Not.Null);
		}

		[Test]
		public void listeditor_should_contain_correct_users()
		{
			// Arrange

			// Act
			List<UserViewModel> users = _userService.ListEditors().ToList();

			// Assert
			Assert.That(users.Count, Is.EqualTo(2));
			Assert.That(users.FirstOrDefault(u => u.ExistingUsername == "editor1"), Is.Not.Null);
			Assert.That(users.FirstOrDefault(u => u.ExistingUsername == "editor2"), Is.Not.Null);
		}

		[Test]
		public void should_not_throw_securityexception_with_valid_ldap_string()
		{
			// Arrange + Act
			ActiveDirectoryUserService manager = new ActiveDirectoryUserService(_applicationSettings, _repository, _adProviderMock.Object);

			// Assert
			Assert.That(_userService, Is.Not.Null);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void Empty_Ldap_String_Should_Throw_SecurityException_In_Constructor()
		{
			// Arrange + act + assert
			_applicationSettings.LdapConnectionString = "";
			ActiveDirectoryUserService service = new ActiveDirectoryUserService(_applicationSettings, _repository, _adProviderMock.Object);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void Wrong_Format_Ldap_String_Should_Throw_SecurityException_In_Constructor()
		{
			// Arrange + act + assert
			_applicationSettings.LdapConnectionString = "iforgot.the.ldap.part.com";
			ActiveDirectoryUserService service = new ActiveDirectoryUserService(_applicationSettings, _repository, _adProviderMock.Object);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void No_Admin_Group_Should_Throw_SecurityException_In_Constructor()
		{
			// Arrange + act + assert
			_applicationSettings.AdminRoleName = "";
			ActiveDirectoryUserService service = new ActiveDirectoryUserService(_applicationSettings, _repository, _adProviderMock.Object);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void No_Editor_Group_Should_Throw_SecurityException_In_Constructor()
		{
			// Arrange + act + assert
			_applicationSettings.EditorRoleName = "";
			ActiveDirectoryUserService service = new ActiveDirectoryUserService(_applicationSettings, _repository, _adProviderMock.Object);
		}
	}
}
#endif