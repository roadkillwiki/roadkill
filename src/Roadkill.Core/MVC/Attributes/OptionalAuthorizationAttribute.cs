using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using StructureMap.Attributes;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Describes a page that doesn't require a login to view, unless Roadkill has IsPublicSite=false. 
	/// </summary>
	public class OptionalAuthorizationAttribute : AuthorizeAttribute, IControllerAttribute
	{
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		[SetterProperty]
		public IUserContext Context { get; set; }

		[SetterProperty]
		public UserManagerBase UserManager { get; set; }

		[SetterProperty]
		public PageManager PageManager { get; set; }

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
			if (!ApplicationSettings.Installed || ApplicationSettings.UpgradeRequired)
			{
				return true;
			}

			IPrincipal user = httpContext.User;
			IIdentity identity = user.Identity;

			// If the site is private then check for a login
			if (!ApplicationSettings.IsPublicSite)
			{
				if (!identity.IsAuthenticated)
				{
					return false;
				}
				else
				{
					if (UserManager.IsAdmin(identity.Name) || UserManager.IsEditor(identity.Name))
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
