using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Configuration;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Describes a page that doesn't require a login to view, unless Roadkill has IsPublicSite=false. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class OptionalAuthorizationAttribute : Attribute, IAuthorizationFilter, IAuthorizationAttribute
	{
		/// <summary>
		/// The authorization provider. If this isn't set, it's taken from the request's services.
		/// </summary>
		public IAuthorizationProvider AuthorizationProvider { get; set; }

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			// [AllowAnonymous] actions skip the check, as they did with the ASP.NET MVC AuthorizeAttribute.
			if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any())
				return;

			ApplicationSettings applicationSettings = context.HttpContext.RequestServices.GetRequiredService<ApplicationSettings>();

			if (!applicationSettings.Installed || applicationSettings.IsPublicSite)
				return;

			// If the site is private then check for a login
			IAuthorizationProvider provider = AuthorizationProvider ?? context.HttpContext.RequestServices.GetService<IAuthorizationProvider>();
			if (provider == null)
				throw new SecurityException("The AuthorizationProvider property has not been set for OptionalAuthorizationAttribute.", null);

			if (!(provider.IsAdmin(context.HttpContext.User) || provider.IsEditor(context.HttpContext.User)))
				context.Result = new ChallengeResult();
		}
	}
}
