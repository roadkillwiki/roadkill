using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Roadkill.Tests.Unit.Attributes
{
	public class PrincipalMock : IPrincipal
	{
		IIdentity IPrincipal.Identity { get { return Identity; } }
		public IdentityStub Identity { get; set; }

		public PrincipalMock()
		{
			Identity = new IdentityStub();
		}

		public bool IsInRole(string role)
		{
			throw new NotImplementedException();
		}
	}

}
