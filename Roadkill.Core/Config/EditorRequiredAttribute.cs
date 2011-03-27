using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Roadkill.Core
{
	/// <summary>
	/// Extends the Authorize attribute to check if the current user is in the RoadkillSettings.EditorRoleName.
	/// If the current user is an admin, they will also be authorised.
	/// </summary>
	public class EditorRequiredAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			IPrincipal user = httpContext.User;
			IIdentity identity = user.Identity;

			if (!identity.IsAuthenticated)
			{
				return false;
			}

			// An empty editor role name implies everyone is an editor - there's no page security.
			if (string.IsNullOrEmpty(RoadkillSettings.EditorRoleName))
				return true;

			if (System.Web.Security.Roles.IsUserInRole(identity.Name, RoadkillSettings.EditorRoleName) ||
				System.Web.Security.Roles.IsUserInRole(identity.Name, RoadkillSettings.AdminRoleName))
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
