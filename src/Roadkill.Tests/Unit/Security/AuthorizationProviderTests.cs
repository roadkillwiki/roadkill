using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class AuthorizationProviderTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private UserServiceMock _userService;
		private SettingsService _settingsService;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;

			_applicationSettings.AdminRoleName = "Admin";
			_applicationSettings.EditorRoleName = "Editor";
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Should_Throw_Argument_Null_Exception_For_Null_ApplicationSettings()
		{
			// Arrange + Act + Assert
			AuthorizationProvider provider = new AuthorizationProvider(null, _userService);
		}

		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Should_Throw_Argument_Null_Exception_For_Null_UserService()
		{
			// Arrange + Act + Assert
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, null);
		}

		//
		// Viewers
		//

		[Test]
		public void IsViewer_Should_Return_True_When_Not_Authenticated()
		{
			// Arrange
			User editorUser = CreateEditorUser();
			IdentityStub identity = new IdentityStub() { IsAuthenticated = false };
			IPrincipal principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsViewer(principal);

			// Assert
			Assert.That(isAuthenticated, Is.True);
		}

		//
		// Admins
		//

		[Test]
		public void IsAdmin_Should_Return_True_For_Admin_User()
		{
			// Arrange
			User adminUser = CreateAdminUser();
			IdentityStub identity = new IdentityStub() { Name = adminUser.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsAdmin(principal);

			// Assert
			Assert.That(isAuthenticated, Is.True);
		}

		[Test]
		public void IsAdmin_Should_Return_False_For_Editor_User()
		{
			// Arrange
			User editorUser = CreateEditorUser();
			IdentityStub identity = new IdentityStub() { Name = editorUser.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsAdmin(principal);

			// Assert
			Assert.That(isAuthenticated, Is.False);
		}

		[Test]
		public void IsAdmin_Should_Return_False_When_Not_Authenticated()
		{
			// Arrange
			User adminUser = CreateAdminUser();
			IdentityStub identity = new IdentityStub() { Name = adminUser.Id.ToString(), IsAuthenticated = false	};
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsAdmin(principal);

			// Assert
			Assert.That(isAuthenticated, Is.False);
		}

		[Test]
		public void IsAdmin_Should_Return_True_When_No_Admin_Role_Set()
		{
			// Arrange
			_applicationSettings.AdminRoleName = "";

			User adminUser = CreateAdminUser();
			IdentityStub identity = new IdentityStub() { Name = adminUser.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsAdmin(principal);

			// Assert
			Assert.That(isAuthenticated, Is.True);
		}

		[Test]
		public void IsAdmin_Should_Return_False_When_No_Identity_Name_Set()
		{
			// Arrange
			User adminUser = CreateAdminUser();
			IdentityStub identity = new IdentityStub() { Name = "", IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsAdmin(principal);

			// Assert
			Assert.That(isAuthenticated, Is.False);
		}
		
		//
		// Editors
		// 

		[Test]
		public void IsEditor_Should_Return_True_For_Editor_User()
		{
			// Arrange
			User editorUser = CreateEditorUser();
			IdentityStub identity = new IdentityStub() { Name = editorUser.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsEditor(principal);

			// Assert
			Assert.That(isAuthenticated, Is.True);
		}

		[Test]
		public void IsEditor_Should_Return_True_For_Admin_User()
		{
			// Arrange
			User adminUser = CreateAdminUser();
			IdentityStub identity = new IdentityStub() { Name = adminUser.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsEditor(principal);

			// Assert
			Assert.That(isAuthenticated, Is.True);
		}

		[Test]
		public void IsEditor_Should_Return_False_When_Not_Authenticated()
		{
			// Arrange
			User editorUser = CreateEditorUser();
			IdentityStub identity = new IdentityStub() { Name = editorUser.Id.ToString(), IsAuthenticated = false };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsEditor(principal);

			// Assert
			Assert.That(isAuthenticated, Is.False);
		}

		[Test]
		public void IsEditor_Should_Return_True_When_No_Editor_Role_Set()
		{
			// Arrange
			_applicationSettings.EditorRoleName = "";

			User editorUser = CreateEditorUser();
			IdentityStub identity = new IdentityStub() { Name = editorUser.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsEditor(principal);

			// Assert
			Assert.That(isAuthenticated, Is.True);
		}

		[Test]
		public void IsEditor_Should_Return_False_When_No_Identity_Name_Set()
		{
			// Arrange
			User adminUser = CreateAdminUser();
			IdentityStub identity = new IdentityStub() { Name = "", IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsEditor(principal);

			// Assert
			Assert.That(isAuthenticated, Is.False);
		}

		[Test]
		public void IsEditor_Should_Return_False_When_User_Is_Not_Admin_Or_Editor()
		{
			// Arrange
			User user = CreateEditorUser();
			user.IsEditor = false;

			IdentityStub identity = new IdentityStub() { Name = user.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			AuthorizationProvider provider = new AuthorizationProvider(_applicationSettings, _userService);

			// Act
			bool isAuthenticated = provider.IsEditor(principal);

			// Assert
			Assert.That(isAuthenticated, Is.False);
		}

		private User CreateAdminUser()
		{
			_userService.AddUser("admin@localhost", "admin", "password", true, false);
			User user = _userService.GetUser("admin@localhost", false);
			user.IsActivated = true;

			return user;
		}

		private User CreateEditorUser()
		{
			_userService.AddUser("editor@localhost", "editor", "password", false, true);
			User user = _userService.GetUser("editor@localhost", false);
			user.IsActivated = true;

			return user;
		}
	}
}
