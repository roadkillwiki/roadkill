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
	public class EditorRequiredAttributeTests : AuthorizeAttributeBase
	{
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
			attribute.UserManager.AddUser("admin@localhost", "admin", "password", true, true);

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = "admin@localhost";
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
			attribute.UserManager.AddUser("editor@localhost", "editor", "password", false, true);

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = "editor@localhost";
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
			attribute.UserManager.AddUser("weirdlogin@localhost", "editor", "password", false, false);

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = "editor@localhost";
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

			attribute.Context = new UserContext(new UserManagerMock());
			attribute.UserManager = new UserManagerMock();

			return attribute;
		}
	}
}