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
	public class OptionalAuthorizationAttributeTests : AuthorizeAttributeBase
	{
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
		public void Should_Authorize_When_User_Is_Editor_And_Site_Is_Not_Public()
		{
			// Arrange
			OptionalAuthorizationCaller attribute = GetOptionalAuthorizationCaller();
			attribute.ApplicationSettings.IsPublicSite = false;
			attribute.UserManager.AddUser("editor@localhost", "editor", "password", false, true);

			PrincipalMock principal = GetPrincipal();
			principal.Identity.Name = "editor@localhost";
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

			attribute.Context = new UserContext(new UserManagerMock());
			attribute.UserManager = new UserManagerMock();

			return attribute;
		}
	}
}