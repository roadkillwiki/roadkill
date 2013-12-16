using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Security;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class AuthorizationProviderMock : IAuthorizationProvider
	{
		public bool IsAdminResult { get; set; }
		public bool IsEditorResult { get; set; }
		public bool IsViewerResult { get; set; }

		public bool IsAdmin(IPrincipal principal)
		{
			return IsAdminResult;
		}

		public bool IsEditor(IPrincipal principal)
		{
			return IsEditorResult;
		}

		public bool IsViewer(IPrincipal principal)
		{
			return IsViewerResult;
		}
	}
}
