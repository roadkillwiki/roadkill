using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Configuration;
using Roadkill.Core.Exceptions;

namespace Roadkill.Core.Mvc
{
	/// <summary>
	/// Redirects requests to the installer when Roadkill isn't installed (static files are served before this
	/// middleware), and turns <see cref="HttpStatusException"/>s
	/// into HTTP status codes (so the status code pages are shown).
	/// </summary>
	public class InstallCheckMiddleware
	{
		private readonly RequestDelegate _next;

		public InstallCheckMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			ApplicationSettings appSettings = context.RequestServices.GetRequiredService<ApplicationSettings>();

			if (!appSettings.Installed && !IsInstallerRequest(context))
			{
				context.Response.Redirect(context.Request.PathBase + "/install/");
				return;
			}

			try
			{
				await _next(context);
			}
			catch (HttpStatusException ex) when (!context.Response.HasStarted)
			{
				context.Response.Clear();
				context.Response.StatusCode = ex.StatusCode;
			}
		}

		private static bool IsInstallerRequest(HttpContext context)
		{
			PathString path = context.Request.Path;
			return path.StartsWithSegments("/install", StringComparison.OrdinalIgnoreCase)
				|| path.StartsWithSegments("/configurationtester", StringComparison.OrdinalIgnoreCase)
				|| path.StartsWithSegments("/assets", StringComparison.OrdinalIgnoreCase);
		}
	}
}
