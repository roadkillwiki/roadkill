using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Represents an attribute that is used to restrict access by callers to users that are in Editor role group.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class EditorRequiredAttribute : Attribute, IAuthorizationFilter, IAuthorizationAttribute
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

			IAuthorizationProvider provider = AuthorizationProvider ?? context.HttpContext.RequestServices.GetService<IAuthorizationProvider>();
			if (provider == null)
				throw new SecurityException("The AuthorizationProvider property has not been set for EditorRequiredAttribute.", null);

			if (!provider.IsEditor(context.HttpContext.User))
				context.Result = new ChallengeResult();
		}
	}
}
