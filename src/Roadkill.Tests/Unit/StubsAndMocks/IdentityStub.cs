using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class IdentityStub : IIdentity
	{
		public string AuthenticationType { get; set; }
		public bool IsAuthenticated { get; set; }
		public string Name { get; set; }
	}
}
