using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Mvc
{
	/// <summary>
	/// Sets the UI culture for the request from the UI language setting (previously the globalization section of the web.config).
	/// </summary>
	public class UiCultureMiddleware
	{
		private readonly RequestDelegate _next;

		public UiCultureMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public Task InvokeAsync(HttpContext context)
		{
			ApplicationSettings settings = context.RequestServices.GetRequiredService<ApplicationSettings>();

			if (!string.IsNullOrEmpty(settings.UiLanguage))
			{
				try
				{
					CultureInfo.CurrentUICulture = new CultureInfo(settings.UiLanguage);
				}
				catch (CultureNotFoundException ex)
				{
					Log.Warn(ex, "The UI language {0} is not a valid culture", settings.UiLanguage);
				}
			}

			return _next(context);
		}
	}
}
