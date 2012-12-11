using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Domain;
using StructureMap;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Represents an attribute that is used to restrict access by callers to users that are in Editor role group.
	/// </summary>
	public class OptionalAuthorizationAttribute : AuthorizeAttribute
	{
		private IConfigurationContainer _config;
		private UserManager _userManager;

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			Roadkill.Core.Controllers.ControllerBase controller = filterContext.Controller as Roadkill.Core.Controllers.ControllerBase;
			if (controller != null)
			{
				_config = controller.Configuration;
				_userManager = controller.UserManager;
			}

			base.OnAuthorization(filterContext);
		}

		/// <summary>
		/// Provides an entry point for custom authorization checks.
		/// </summary>
		/// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
		/// <returns>
		/// true if the user is an admin, in the role name specified by the roadkill web.config editorRoleName setting or if this is blank; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="httpContext"/> parameter is null.</exception>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (!_config.ApplicationSettings.Installed)
			{
				return true;
			}

			IPrincipal user = httpContext.User;
			IIdentity identity = user.Identity;

			// If the site is private then check for a login
			if (!_config.ApplicationSettings.IsPublicSite)
			{
				if (!identity.IsAuthenticated)
				{
					return false;
				}
				else
				{
					if (_userManager.IsAdmin(identity.Name) || _userManager.IsEditor(identity.Name))
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			else
			{
				return true;
			}
		}
	}
}
