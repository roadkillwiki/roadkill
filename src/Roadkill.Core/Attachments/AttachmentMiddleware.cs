using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Configuration;
using Roadkill.Core.Exceptions;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// Middleware that serves all uploaded files from the attachments route (e.g. /Attachments/foo.jpg).
	/// This replaces the ASP.NET AttachmentRouteHandler and IHttpHandler.
	/// </summary>
	public class AttachmentMiddleware
	{
		private readonly RequestDelegate _next;

		public AttachmentMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			ApplicationSettings settings = context.RequestServices.GetRequiredService<ApplicationSettings>();

			if (!settings.Installed || !IsAttachmentRequest(context.Request.Path, settings))
			{
				await _next(context);
				return;
			}

			// On a private site the attachments need a login, as the pages do (see OptionalAuthorizationAttribute). This middleware
			// runs after the authentication middleware, so context.User is the logged in user.
			if (!settings.IsPublicSite)
			{
				IAuthorizationProvider authorizationProvider = context.RequestServices.GetRequiredService<IAuthorizationProvider>();
				if (!(authorizationProvider.IsAdmin(context.User) || authorizationProvider.IsEditor(context.User)))
				{
					if (context.User?.Identity?.IsAuthenticated == true)
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
					else
						await context.ChallengeAsync(); // redirects to the login page

					return;
				}
			}

			IFileService fileService = context.RequestServices.GetRequiredService<IFileService>();
			ResponseWrapper wrapper = new ResponseWrapper(context.Response) { IsPrivate = !settings.IsPublicSite };

			try
			{
				string localPath = context.Request.PathBase.Add(context.Request.Path).Value;
				string applicationPath = context.Request.PathBase.HasValue ? context.Request.PathBase.Value : "/";

				fileService.WriteResponse(localPath, applicationPath, context.Request.Headers["If-Modified-Since"], wrapper);
				await wrapper.FlushAsync();
			}
			catch (HttpStatusException ex)
			{
				context.Response.StatusCode = ex.StatusCode;
			}
		}

		/// <summary>
		/// Whether the request path is for the attachments route, e.g. /Attachments/foo.jpg.
		/// </summary>
		public static bool IsAttachmentRequest(PathString path, ApplicationSettings settings)
		{
			if (string.IsNullOrEmpty(settings.AttachmentsRoutePath))
				return false;

			// Case sensitive to match the previous route behaviour, and /Attachments on its own isn't a file.
			return path.StartsWithSegments("/" + settings.AttachmentsRoutePath, StringComparison.Ordinal, out PathString remaining)
				&& remaining.HasValue && remaining.Value.Length > 1;
		}

		/// <summary>
		/// Validates the attachments route in the settings.
		/// </summary>
		/// <exception cref="ConfigurationException">The configuration is missing an attachments route path, or it is set to 'files'.</exception>
		public static void ValidateRoute(ApplicationSettings settings)
		{
			if (string.IsNullOrEmpty(settings.AttachmentsRoutePath))
				throw new ConfigurationException("The configuration is missing an attachments route path, please enter one using attachmentsRoutePath=\"Attachments\"", null);

			if (settings.AttachmentsRoutePath.ToLower() == "files")
				throw new ConfigurationException("The attachmentsRoutePath in the config is set to 'files' which is not an allowed route path. Please change it to something else.", null);
		}
	}
}
