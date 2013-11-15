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
	/// Setup-heavy tests for the EditorAdminRequired attribute.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class EditorRequiredAttributeTests : AuthorizeAttributeTestBase
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
		public void Should_Not_Authorize_When_User_Is_Not_Authenticated()
		{
			// Arrange
			EditorRequiredCaller attribute = GetEditorRequiredCaller();
			PrincipalMock principal = GetPrincipal();
			principal.Identity.IsAuthenticated = false;

			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.False);
		}

		[Test]
		public void Should_Authorize_When_No_Editor_Group_Name()
		{
			// Arrange
			EditorRequiredCaller attribute = GetEditorRequiredCaller();
			attribute.ApplicationSettings.EditorRoleName = "";

			PrincipalMock principal = GetPrincipal();
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Authorize_When_Admin()
		{
			// Arrange
			EditorRequiredCaller attribute = GetEditorRequiredCaller();

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = _adminId.ToString();
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Authorize_When_Editor()
		{
			// Arrange
			EditorRequiredCaller attribute = GetEditorRequiredCaller();

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = _editorId.ToString();
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		[Test]
		public void Should_Not_Authorize_When_Not_Editor_Or_Admin()
		{
			// Arrange
			EditorRequiredCaller attribute = GetEditorRequiredCaller();
			attribute.UserService.AddUser("weirdlogin@localhost", "editor", "password", false, false);

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = Guid.NewGuid().ToString();
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.False);
		}

		private EditorRequiredCaller GetEditorRequiredCaller()
		{
			EditorRequiredCaller attribute = new EditorRequiredCaller();
			attribute.ApplicationSettings = new ApplicationSettings();
			attribute.ApplicationSettings.AdminRoleName = "admin";
			attribute.ApplicationSettings.EditorRoleName = "editor";

			attribute.Context = _context;
			attribute.UserService = _userService;

			return attribute;
		}
	}
}