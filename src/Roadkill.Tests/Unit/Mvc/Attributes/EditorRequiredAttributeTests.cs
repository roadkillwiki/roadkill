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

namespace Roadkill.Tests.Unit
{
	/// <summary>
	/// Setup-heavy tests for the AdminRequired attribute.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class EditorRequiredAttributeTests
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
		public void Should_Use_AuthorizationProvider()
		{
			// Arrange
			EditorRequiredAttributeMock attribute = new EditorRequiredAttributeMock();
			attribute.AuthorizationProvider = new AuthorizationProviderMock() { IsEditorResult = true };
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
		[ExpectedException(typeof(SecurityException))]
		public void Should_Throw_SecurityException_When_AuthorizationProvider_Is_Null()
		{
			// Arrange
			EditorRequiredAttributeMock attribute = new EditorRequiredAttributeMock();
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
	}

	internal class EditorRequiredAttributeMock : EditorRequiredAttribute
	{
		public bool CallAuthorize(HttpContextBase context)
		{
			return base.AuthorizeCore(context);
		}
	}
}