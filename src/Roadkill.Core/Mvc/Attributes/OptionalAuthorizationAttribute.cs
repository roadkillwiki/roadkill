using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using StructureMap.Attributes;
using Roadkill.Core.DI;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Describes a page that doesn't require a login to view, unless Roadkill has IsPublicSite=false. 
	/// </summary>
	public class OptionalAuthorizationAttribute : AuthorizeAttribute, ISetterInjected
	{
		[SetterProperty]
		public ApplicationSettings ApplicationSettings { get; set; }

		[SetterProperty]
		public IUserContext Context { get; set; }

		[SetterProperty]
		public UserServiceBase UserService { get; set; }

		[SetterProperty]
		public IPageService PageService { get; set; }

		[SetterProperty]
		public SettingsService SettingsService { get; set; }

		[SetterProperty]
		public IAuthorizationProvider AuthorizationProvider { get; set; }

		/// <summary>
		/// Provides an entry point for custom authorization checks.
		/// </summary>
		/// <param name="httpContext">The HTTP context, which encapsulates all HTTP-specific information about an individual HTTP request.</param>
		/// <returns>
		/// false if the user is an admin or editor AND the site is private (ispublicsite=false). Otherwise true is returned.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="httpContext"/> parameter is null.</exception>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (AuthorizationProvider == null)
				throw new SecurityException("The OptionalAuthorizationAttribute property has not been set for AdminRequiredAttribute.", null);

			if (!ApplicationSettings.Installed || ApplicationSettings.UpgradeRequired)
			{
				return true;
			}

			// If the site is private then check for a login
			if (!ApplicationSettings.IsPublicSite)
			{
				IPrincipal principal = httpContext.User;

				AuthorizationProvider provider = new AuthorizationProvider(ApplicationSettings, UserService);
				return provider.IsAdmin(principal) || provider.IsEditor(principal);
			}
			else
			{
				return true;
			}
		}
	}
}