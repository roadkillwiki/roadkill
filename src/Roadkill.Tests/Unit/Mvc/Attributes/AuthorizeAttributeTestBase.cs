using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Moq;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Tests.Unit.Attributes;

namespace Roadkill.Tests.Unit
{
	public abstract class AuthorizeAttributeTestBase
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

	/// A slightly less messy (non-Moq) way of calling the base attribute classes.

	public class EditorRequiredCaller : EditorRequiredAttribute
	{
		public bool CallAuthorize(HttpContextBase context)
		{
			return base.AuthorizeCore(context);
		}
	}

	public class AdminRequiredCaller : AdminRequiredAttribute
	{
		public bool CallAuthorize(HttpContextBase context)
		{
			return base.AuthorizeCore(context);
		}
	}

	public class OptionalAuthorizationCaller : OptionalAuthorizationAttribute
	{
		public bool CallAuthorize(HttpContextBase context)
		{
			return base.AuthorizeCore(context);
		}
	}
}
