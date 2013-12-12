using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Security.Windows
{
	/// <summary>
	/// Wraps information needed from an <see cref="IPrincipal"/>
	/// </summary>
	public interface IPrincipalDetails
	{
		string SamAccountName { get; set; }
	}
}
