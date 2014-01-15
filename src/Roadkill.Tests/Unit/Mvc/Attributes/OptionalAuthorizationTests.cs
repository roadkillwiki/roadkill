using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Mvc.Controllers;
using MvcContrib.TestHelper;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Attributes;
using System.Security.Principal;
using Roadkill.Core.Security;
using Roadkill.Tests.Unit.StubsAndMocks;
using System.Web.Http.Controllers;
using System.Threading;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit
{
	/// <summary>
	/// Setup-heavy tests for the AdminRequired attribute.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class OptionalAuthorizationTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private UserServiceMock _userService;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_userService = _container.UserService;

			_applicationSettings.AdminRoleName = "Admin";
			_applicationSettings.EditorRoleName = "Editor";
		}

		[Test]
		public void Should_Return_True_If_Installed_Is_False()
		{
			// Arrange
			_applicationSettings.Installed = false;

			OptionalAuthorizationAttributeMock attribute = new OptionalAuthorizationAttributeMock();
			attribute.AuthorizationProvider = new AuthorizationProviderMock();
			attribute.ApplicationSettings = _applicationSettings;
			attribute.UserService = _userService;

			IdentityStub identity = new IdentityStub() { Name = Guid.NewGuid().ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Return_True_If_UpgradeRequired_Is_True()
		{
			// Arrange
			_applicationSettings.UpgradeRequired = true;

			OptionalAuthorizationAttributeMock attribute = new OptionalAuthorizationAttributeMock();
			attribute.AuthorizationProvider = new AuthorizationProviderMock();
			attribute.ApplicationSettings = _applicationSettings;
			attribute.UserService = _userService;

			IdentityStub identity = new IdentityStub() { Name = Guid.NewGuid().ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Return_True_If_PublicSite_Is_True()
		{
			// Arrange
			_applicationSettings.IsPublicSite = true;

			OptionalAuthorizationAttributeMock attribute = new OptionalAuthorizationAttributeMock();
			attribute.AuthorizationProvider = new AuthorizationProviderMock();
			attribute.ApplicationSettings = _applicationSettings;
			attribute.UserService = _userService;

			IdentityStub identity = new IdentityStub() { Name = Guid.NewGuid().ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Use_AuthorizationProvider_For_Editors_When_PublicSite_Is_False()
		{
			// Arrange
			User editorUser = CreateEditorUser();

			OptionalAuthorizationAttributeMock attribute = new OptionalAuthorizationAttributeMock();
			attribute.AuthorizationProvider = new AuthorizationProviderMock() { IsEditorResult = true };
			attribute.ApplicationSettings = _applicationSettings;
			attribute.UserService = _userService;

			IdentityStub identity = new IdentityStub() { Name = editorUser.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Use_AuthorizationProvider_For_Admin_When_PublicSite_Is_False()
		{
			// Arrange
			User adminUser = CreateAdminUser();

			OptionalAuthorizationAttributeMock attribute = new OptionalAuthorizationAttributeMock();
			attribute.AuthorizationProvider = new AuthorizationProviderMock() { IsEditorResult = true };
			attribute.ApplicationSettings = _applicationSettings;
			attribute.UserService = _userService;

			IdentityStub identity = new IdentityStub() { Name = adminUser.Id.ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		[ExpectedException(typeof(SecurityException))]
		public void Should_Throw_SecurityException_When_AuthorizationProvider_Is_Null()
		{
			// Arrange
			OptionalAuthorizationAttributeMock attribute = new OptionalAuthorizationAttributeMock();
			attribute.AuthorizationProvider = null;

			IdentityStub identity = new IdentityStub() { Name = Guid.NewGuid().ToString(), IsAuthenticated = true };
			PrincipalStub principal = new PrincipalStub() { Identity = identity };
			HttpContextBase context = GetHttpContext(principal);

			// Act + Assert
			attribute.CallAuthorize(context);
		}

		protected HttpContextBase GetHttpContext(PrincipalStub principal)
		{
			MvcMockContainer container = new MvcMockContainer();
			HttpContextBase context = MvcMockHelpers.FakeHttpContext(container);
			container.Context.SetupProperty(x => x.User, principal);

			return context;
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

	internal class OptionalAuthorizationAttributeMock : OptionalAuthorizationAttribute
	{
		public bool CallAuthorize(HttpContextBase context)
		{
			return base.AuthorizeCore(context);
		}
	}
}