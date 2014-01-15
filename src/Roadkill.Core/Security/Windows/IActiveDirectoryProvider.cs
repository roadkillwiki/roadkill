using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Security.Windows
{
	/// <summary>
	/// Provides group membership lookup for the <see cref="ActiveDirectoryUserService"/>
	/// </summary>
	public interface IActiveDirectoryProvider
	{
		IEnumerable<IPrincipalDetails> GetMembers(string domainName, string username, string password, string groupName);
		string TestLdapConnection(string connectionString, string username, string password, string groupName);
	}
}
