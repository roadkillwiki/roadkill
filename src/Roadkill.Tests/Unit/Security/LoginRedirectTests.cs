using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Roadkill.Core.DependencyResolution;

namespace Roadkill.Tests.Unit.Security
{
	[TestFixture]
	[Category("Unit")]
	public class LoginRedirectTests
	{
		private static RedirectContext<CookieAuthenticationOptions> CreateContext(HttpContext httpContext, string redirectUri)
		{
			var scheme = new AuthenticationScheme(CookieAuthenticationDefaults.AuthenticationScheme, null, typeof(CookieAuthenticationHandler));
			return new RedirectContext<CookieAuthenticationOptions>(httpContext, scheme, new CookieAuthenticationOptions(), new AuthenticationProperties(), redirectUri);
		}

		[Test]
		public void login_redirect_should_be_relative_to_keep_the_browser_scheme_and_host()
		{
			// Arrange (behind a reverse proxy that ends https, the absolute url built by ASP.NET Core is http)
			var httpContext = new DefaultHttpContext();
			var context = CreateContext(httpContext, "http://wiki.example.local:9443/user/login?ReturnUrl=%2F");

			// Act
			RoadkillServiceCollectionExtensions.RedirectToRelativeUri(context);

			// Assert
			Assert.That(httpContext.Response.StatusCode, Is.EqualTo(302));
			Assert.That(httpContext.Response.Headers.Location.ToString(), Is.EqualTo("/user/login?ReturnUrl=%2F"));
		}

		[Test]
		public void ajax_requests_should_get_a_401_with_the_relative_login_url()
		{
			// Arrange
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
			var context = CreateContext(httpContext, "http://wiki.example.local:9443/user/login?ReturnUrl=%2Ffilemanager");

			// Act
			RoadkillServiceCollectionExtensions.RedirectToRelativeUri(context);

			// Assert
			Assert.That(httpContext.Response.StatusCode, Is.EqualTo(401));
			Assert.That(httpContext.Response.Headers.Location.ToString(), Is.EqualTo("/user/login?ReturnUrl=%2Ffilemanager"));
		}
	}
}
