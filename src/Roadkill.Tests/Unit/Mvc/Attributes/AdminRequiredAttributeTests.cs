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

namespace Roadkill.Tests.Unit
{
	public abstract class AuthorizeAttributeTests
	{
		protected PrincipalMock GetPrincipal()
		{
			PrincipalMock principal = new PrincipalMock();
			principal.Identity.IsAuthenticated = true;

			return principal;
		}

		protected HttpContextBase GetHttpContext(PrincipalMock principal)
		{
			MvcMockContainer container = new MvcMockContainer();
			HttpContextBase context = MvcMockHelpers.FakeHttpContext(container);
			container.Context.SetupProperty(x => x.User, principal);

			return context;
		}
	}

	[TestFixture]
	[Category("Unit")]
	public class AdminRequiredAttributeTests : AuthorizeAttributeTests
	{
		[Test]
		public void Should_Not_Authorize_If_User_Is_Not_Authenticated()
		{
			// Arrange
			AdminRequiredMock attribute = GetAdminRequiredMock();
			PrincipalMock principal = GetPrincipal();
			principal.Identity.IsAuthenticated = false;

			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.False);
		}

		[Test]
		public void Should_Authorize_If_No_Admin_Group_Name()
		{
			// Arrange
			AdminRequiredMock attribute = GetAdminRequiredMock();
			attribute.ApplicationSettings.AdminRoleName = "";

			PrincipalMock principal = GetPrincipal();
			HttpContextBase context = GetHttpContext(principal);

			// Act
			bool isAuthorized = attribute.CallAuthorize(context);

			// Assert
			Assert.That(isAuthorized, Is.True);
		}

		private AdminRequiredMock GetAdminRequiredMock()
		{
			AdminRequiredMock attribute = new AdminRequiredMock();
			attribute.ApplicationSettings = new ApplicationSettings();
			attribute.Context = new UserContext(new UserManagerMock());
			attribute.UserManager = new UserManagerMock();

			return attribute;
		}
	}

	public class AdminRequiredMock : AdminRequiredAttribute
	{
		public bool CallAuthorize(HttpContextBase context)
		{
			return base.AuthorizeCore(context);
		}
	}

	public class PrincipalMock : IPrincipal
	{
		public PrincipalMock()
		{
			Identity = new MyIdentityMock();
		}

		IIdentity IPrincipal.Identity { get { return Identity; } }

		public MyIdentityMock Identity { get; set; }

		public bool IsInRole(string role)
		{
			throw new NotImplementedException();
		}
	}

	public class MyIdentityMock : IIdentity
	{
		public string AuthenticationType
		{
			get { return "authtype"; }
		}

		public bool IsAuthenticated { get; set; }

		public string Name
		{
			get { return "name"; }
		}
	}
}