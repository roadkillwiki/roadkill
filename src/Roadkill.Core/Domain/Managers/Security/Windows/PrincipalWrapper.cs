using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	internal class PrincipalWrapper : IRoadKillPrincipal
	{
		public string SamAccountName { get; set; }

		public PrincipalWrapper(UserPrincipal principal)
		{
			SamAccountName = principal.SamAccountName;
		}
	}
}
