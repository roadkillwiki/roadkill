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
	/// Represents an attribute that is used to restrict access by callers to users that are in Admin role group.
	/// </summary>
	public class AdminRequiredAttribute : AuthorizeAttribute, IControllerAttribute
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
		/// true if the user is in the role name specified by the roadkill web.config adminRoleName setting or if this is blank; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="httpContext"/> parameter is null.</exception>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			IPrincipal user = httpContext.User;
			IIdentity identity = user.Identity;

			if (!identity.IsAuthenticated)
			{
				return false;
			}

			if (string.IsNullOrEmpty(ApplicationSettings.AdminRoleName))
				return true;

			if (UserManager.IsAdmin(identity.Name))
				return true;
			else
				return false;
		}
	}
}
