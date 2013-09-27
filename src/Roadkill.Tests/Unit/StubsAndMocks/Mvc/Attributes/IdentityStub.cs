using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace Roadkill.Tests.Unit.Attributes
{
	public class IdentityStub : IIdentity
	{
		public string AuthenticationType
		{
			get { return "authtype"; }
		}

		public bool IsAuthenticated { get; set; }

		string IIdentity.Name { get { return Name; } }
		public string Name { get; set; }
	}
}
