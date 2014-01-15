using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Security.Windows;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class ActiveDirectoryProviderMock : IActiveDirectoryProvider
	{
		public string LdapConnectionResult { get; set; }

		public IEnumerable<IPrincipalDetails> GetMembers(string domainName, string username, string password, string groupName)
		{
			return new List<IPrincipalDetails>();
		}

		public string TestLdapConnection(string connectionString, string username, string password, string groupName)
		{
			return LdapConnectionResult;
		}
	}
}
