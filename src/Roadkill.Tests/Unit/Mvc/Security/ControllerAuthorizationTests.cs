using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.WebApi;

namespace Roadkill.Tests.Unit.Mvc.Security
{
	/// <summary>
	/// Guards against a controller that shows content without the private site check: every controller must require a
	/// login on a private site (OptionalAuthorization), editor/admin rights, or an API key, except the ones listed here.
	/// </summary>
	[TestFixture]
	[Category("Unit")]
	public class ControllerAuthorizationTests
	{
		// Controllers that must work without a login: the installer and its tests (they check the installed/admin state
		// themselves), and the login/sign up/profile pages.
		private static readonly string[] _allowedWithoutAuthorization =
		{
			"InstallController",
			"ConfigurationTesterController",
			"UserController"
		};

		[Test]
		public void all_controllers_should_require_a_login_on_a_private_site_or_be_explicitly_allowed()
		{
			// Arrange
			Type[] controllers = typeof(WikiController).Assembly.GetTypes()
				.Where(t => !t.IsAbstract && t.Name.EndsWith("Controller") && typeof(Microsoft.AspNetCore.Mvc.ControllerBase).IsAssignableFrom(t))
				.ToArray();

			// Act
			string[] unprotected = controllers
				.Where(t => t.GetCustomAttribute<OptionalAuthorizationAttribute>(true) == null
						 && t.GetCustomAttribute<EditorRequiredAttribute>(true) == null
						 && t.GetCustomAttribute<AdminRequiredAttribute>(true) == null
						 && t.GetCustomAttribute<ApiKeyAuthorizeAttribute>(true) == null
						 && !_allowedWithoutAuthorization.Contains(t.Name))
				.Select(t => t.FullName)
				.ToArray();

			// Assert
			Assert.That(controllers, Is.Not.Empty);
			Assert.That(unprotected, Is.Empty, "Controllers without authorization: " + string.Join(", ", unprotected));
		}
	}
}
