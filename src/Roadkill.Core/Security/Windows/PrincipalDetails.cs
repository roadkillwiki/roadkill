using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Security.Windows
{
	public class PrincipalDetails : IPrincipalDetails
	{
		public string SamAccountName { get; set; }

		public PrincipalDetails(UserPrincipal principal)
		{
			SamAccountName = principal.SamAccountName;
		}
	}
}
