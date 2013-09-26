using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Security.Windows
{
	/// <summary>
	/// Provides a service for group membership lookup for the <see cref="ActiveDirectoryUserManager"/>
	/// </summary>
	public interface IActiveDirectoryService
	{
		IEnumerable<IPrincipalDetails> GetMembers(string domainName, string username, string password, string groupName);
	}
}
