using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Mvc.WebApi
{
	/// <summary>
	/// Requires a valid API key (from the config file's apiKeys setting) in the Authorization header.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class ApiKeyAuthorizeAttribute : Attribute, IAuthorizationFilter
	{
		public static readonly string APIKEY_HEADER_KEY = "Authorization";

		/// <summary>
		/// The application settings. If this isn't set, they are taken from the request's services.
		/// </summary>
		public ApplicationSettings ApplicationSettings { get; set; }

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			if (!context.HttpContext.Request.Headers.ContainsKey(APIKEY_HEADER_KEY))
			{
				context.Result = new BadRequestResult();
				return;
			}

			ApplicationSettings settings = ApplicationSettings ?? context.HttpContext.RequestServices.GetRequiredService<ApplicationSettings>();
			string keyValue = context.HttpContext.Request.Headers[APIKEY_HEADER_KEY].First();

			if (settings.ApiKeys == null || !settings.ApiKeys.Contains(keyValue))
				context.Result = new UnauthorizedResult();
		}
	}
}
