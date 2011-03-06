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
	/// Extends the Authorize attribute to check if the user is in the RoadkillSettings.AdminGroup.
	/// To enable this, set the 'Role' property to 'Admins'
	/// </summary>
	public class RoadkillAuthorizeAttribute : AuthorizeAttribute
	{
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			IPrincipal user = httpContext.User;
			IIdentity identity = user.Identity;

			if (!identity.IsAuthenticated)
			{
				return false;
			}
			
			if (base.Roles == "Admins")
			{
				if (System.Web.Security.Roles.IsUserInRole(identity.Name, RoadkillSettings.AdminGroup))
					return true;
				else
					return false;
			}
			else
			{
				return true;
			}
		}
	}
}
