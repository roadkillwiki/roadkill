using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Configuration;
using Roadkill.Core.Security;

namespace Roadkill.Core.Security
{
	public class AuthorizationProvider : IAuthorizationProvider
	{
		private readonly ApplicationSettings _applicationSettings;
		private readonly UserServiceBase _userService;

		public AuthorizationProvider(ApplicationSettings applicationSettings, UserServiceBase userService)
		{
			if (applicationSettings == null)
				throw new ArgumentNullException("applicationSettings");

			if (userService == null)
				throw new ArgumentNullException("userService");

			_applicationSettings = applicationSettings;
			_userService = userService;
		}

		public virtual bool IsAdmin(IPrincipal principal)
		{
			IIdentity identity = principal.Identity;

			if (!identity.IsAuthenticated)
			{
				return false;
			}

			// An empty admin role name implies everyone is an admin - there's roles at all.
			if (string.IsNullOrEmpty(_applicationSettings.AdminRoleName))
				return true;

			// For custom IIdentity implementations, check the name (for Windows this should never happen)
			if (string.IsNullOrEmpty(identity.Name))
				return false;

			if (_userService.IsAdmin(identity.Name))
				return true;
			else
				return false;
		}

		public virtual bool IsEditor(IPrincipal principal)
		{
			IIdentity identity = principal.Identity;

			if (!identity.IsAuthenticated)
			{
				return false;
			}

			// An empty editor role name implies everyone is an editor - there's no page security.
			if (string.IsNullOrEmpty(_applicationSettings.EditorRoleName))
				return true;

			// Same as IsAdmin - for custom IIdentity implementations, check the name (for Windows this should never happen)
			if (string.IsNullOrEmpty(identity.Name))
				return false;

			if (_userService.IsAdmin(identity.Name) || _userService.IsEditor(identity.Name))
				return true;
			else
				return false;
		}

		public virtual bool IsViewer(IPrincipal principal)
		{
			return true;
		}
	}
}
