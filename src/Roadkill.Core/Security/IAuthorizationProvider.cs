using System;
using System.Security.Principal;

namespace Roadkill.Core.Security
{
	public interface IAuthorizationProvider
	{
		bool IsAdmin(IPrincipal principal);
		bool IsEditor(IPrincipal principal);
		bool IsViewer(IPrincipal principal);
	}
}
