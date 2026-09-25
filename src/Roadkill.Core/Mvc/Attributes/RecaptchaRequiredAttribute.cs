using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Validates the Google reCAPTCHA (v2) response, if recaptcha is enabled in the site settings. The result is passed
	/// to the action as the "isCaptchaValid" parameter.
	/// </summary>
	public class RecaptchaRequiredAttribute : ActionFilterAttribute
	{
		internal static readonly string ResponseKey = "g-recaptcha-response";
		private static readonly string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";
		private static readonly HttpClient _httpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };

		public override async Task OnActionExecutionAsync(ActionExecutingContext filterContext, ActionExecutionDelegate next)
		{
			SettingsService settingsService = filterContext.HttpContext.RequestServices.GetService<SettingsService>();
			SiteSettings siteSettings = settingsService?.GetSiteSettings();

			if (siteSettings != null && siteSettings.IsRecaptchaEnabled)
			{
				string responseValue = filterContext.HttpContext.Request.HasFormContentType
					? (string)filterContext.HttpContext.Request.Form[ResponseKey]
					: "";

				bool isValid = await IsValidAsync(siteSettings.RecaptchaPrivateKey, responseValue, filterContext.HttpContext.Connection.RemoteIpAddress?.ToString());
				filterContext.ActionArguments["isCaptchaValid"] = isValid;
			}

			await next();
		}

		private static async Task<bool> IsValidAsync(string secret, string response, string remoteIp)
		{
			if (string.IsNullOrEmpty(response))
				return false;

			try
			{
				var content = new FormUrlEncodedContent(new Dictionary<string, string>()
				{
					{ "secret", secret ?? "" },
					{ "response", response },
					{ "remoteip", remoteIp ?? "" }
				});

				HttpResponseMessage httpResponse = await _httpClient.PostAsync(VerifyUrl, content);
				string json = await httpResponse.Content.ReadAsStringAsync();

				using (JsonDocument document = JsonDocument.Parse(json))
				{
					return document.RootElement.TryGetProperty("success", out JsonElement success) && success.GetBoolean();
				}
			}
			catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException || ex is JsonException)
			{
				Log.Error(ex, "Unable to validate the recaptcha response");
				return false;
			}
		}
	}
}
