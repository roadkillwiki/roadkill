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
	public class AdminRequiredAttributeTests : AuthorizeAttributeBase
	{
		[Test]
		public void Should_Not_Authorize_When_User_Is_Not_Authenticated()
		{
			// Arrange
			AdminRequiredCaller attribute = GetAdminRequiredCaller();
			PrincipalMock principal = GetPrincipal();
			principal.Identity.IsAuthenticated = false;

			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.False);
		}

		[Test]
		public void Should_Authorize_When_No_Admin_Group_Name()
		{
			// Arrange
			AdminRequiredCaller attribute = GetAdminRequiredCaller();
			attribute.ApplicationSettings.AdminRoleName = "";

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
			AdminRequiredCaller attribute = GetAdminRequiredCaller();
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
		public void Should_Not_Authorize_When_Editor()
		{
			// Arrange
			AdminRequiredCaller attribute = GetAdminRequiredCaller();
			attribute.UserManager.AddUser("admin@localhost", "admin", "password", true, true);
			attribute.UserManager.AddUser("editor@localhost", "editor", "password", false, true);

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = "editor@localhost";
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.False);
		}

		private AdminRequiredCaller GetAdminRequiredCaller()
		{
			AdminRequiredCaller attribute = new AdminRequiredCaller();
			attribute.ApplicationSettings = new ApplicationSettings();
			attribute.ApplicationSettings.AdminRoleName = "admin";
			attribute.ApplicationSettings.EditorRoleName = "editor";

			attribute.Context = new UserContext(new UserManagerMock());
			attribute.UserManager = new UserManagerMock();

			return attribute;
		}
	}
}