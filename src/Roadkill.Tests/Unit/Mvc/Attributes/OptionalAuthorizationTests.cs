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
using Roadkill.Tests.Unit.Attributes;
using MvcContrib.TestHelper;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Attributes;
using System.Security.Principal;

namespace Roadkill.Tests.Unit
{
	/// <summary>
	/// Setup-heavy tests for the AdminRequired attribute.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class OptionalAuthorizationAttributeTests : AuthorizeAttributeTestBase
	{
		private MocksAndStubsContainer _container;
		private UserServiceMock _userService;
		private IUserContext _context;

		private Guid _adminId;
		private Guid _editorId;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_userService = _container.UserService;
			_context = _container.UserContext;

			_userService.AddUser("admin@localhost", "admin", "password", true, true);
			_userService.AddUser("editor@localhost", "editor", "password", false, true);
			_userService.Users[0].IsActivated = true;
			_userService.Users[1].IsActivated = true;

			_adminId = _userService.Users[0].Id;
			_editorId = _userService.Users[1].Id;
		}

		[Test]
		public void Should_Authorize_When_Not_Installed()
		{
			// Arrange
			OptionalAuthorizationCaller attribute = GetOptionalAuthorizationCaller();
			attribute.ApplicationSettings.Installed = false;

			PrincipalMock principal = GetPrincipal();
			principal.Identity.IsAuthenticated = false;

			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context); // skips authorization

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Authorize_When_Upgrade_Pending()
		{
			// Arrange
			OptionalAuthorizationCaller attribute = GetOptionalAuthorizationCaller();
			attribute.ApplicationSettings.UpgradeRequired = true;

			PrincipalMock principal = GetPrincipal();
			principal.Identity.IsAuthenticated = false;

			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context); // skips authorization

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Authorize_When_Site_Is_Public()
		{
			// Arrange
			OptionalAuthorizationCaller attribute = GetOptionalAuthorizationCaller();
			attribute.ApplicationSettings.IsPublicSite = true;

			PrincipalMock principal = GetPrincipal();
			principal.Identity.IsAuthenticated = false;

			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context); // skips authorization

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Not_Authorize_When_Not_Authenticated_And_Site_Is_Not_Public()
		{
			// Arrange
			OptionalAuthorizationCaller attribute = GetOptionalAuthorizationCaller();
			attribute.ApplicationSettings.IsPublicSite = false;

			PrincipalMock principal = GetPrincipal();
			principal.Identity.IsAuthenticated = false;

			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.False);
		}

		[Test]
		public void Should_Authorize_When_User_Is_Admin_And_Site_Is_Not_Public()
		{
			// Arrange
			OptionalAuthorizationCaller attribute = GetOptionalAuthorizationCaller();
			attribute.ApplicationSettings.IsPublicSite = false;

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = _adminId.ToString();
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Authorize_When_User_Is_Editor_And_Site_Is_Not_Public()
		{
			// Arrange
			OptionalAuthorizationCaller attribute = GetOptionalAuthorizationCaller();
			attribute.ApplicationSettings.IsPublicSite = false;

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = _editorId.ToString();
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		private OptionalAuthorizationCaller GetOptionalAuthorizationCaller()
		{
			OptionalAuthorizationCaller attribute = new OptionalAuthorizationCaller();
			attribute.ApplicationSettings = new ApplicationSettings();
			attribute.ApplicationSettings.Installed = true;
			attribute.ApplicationSettings.AdminRoleName = "admin";
			attribute.ApplicationSettings.EditorRoleName = "editor";

			attribute.Context = _context;
			attribute.UserService = _userService;

			return attribute;
		}
	}
}