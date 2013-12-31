using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Attachments
{
	/// <summary>
	/// A route handler for the attachments virtual folder. This route handler doesn't swallow MVC routes.
	/// </summary>
	public class AttachmentRouteHandler : IRouteHandler
	{
		private ApplicationSettings _settings;

		/// <summary>
		/// Initializes a new instance of the <see cref="AttachmentRouteHandler"/> class.
		/// </summary>
		/// <param name="settings">The current application settings.</param>
		public AttachmentRouteHandler(ApplicationSettings settings)
		{
			_settings = settings;
		}

		/// <summary>
		/// Registers the attachments path route, using the settings given in the application settings.
		/// </summary>
		/// <param name="settings">The settings.</param>
		/// <param name="routes">The routes.</param>
		/// <exception cref="ConfigurationException">
		/// The configuration is missing an attachments route path.
		/// or
		/// The attachmentsRoutePath in the config is set to 'files' which is not an allowed route path.
		/// </exception>
		public static void RegisterRoute(ApplicationSettings settings, RouteCollection routes)
		{
			if (string.IsNullOrEmpty(settings.AttachmentsRoutePath))
				throw new ConfigurationException("The configuration is missing an attachments route path, please enter one using attachmentsRoutePath=\"Attachments\"", null);

			if (settings.AttachmentsRoutePath.ToLower() == "files")
				throw new ConfigurationException("The attachmentsRoutePath in the config is set to 'files' which is not an allowed route path. Please change it to something else.", null);


			Route route = new Route(settings.AttachmentsRoutePath + "/{*filename}", new AttachmentRouteHandler(settings));
			route.Constraints = new RouteValueDictionary();
			route.Constraints.Add("MvcContraint", new IgnoreMvcConstraint(settings));

			routes.Add(route);
		}

		/// <summary>
		/// Provides the object that processes the request.
		/// </summary>
		/// <param name="requestContext">An object that encapsulates information about the request.</param>
		/// <returns>
		/// An object that processes the request.
		/// </returns>
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new AttachmentFileHandler(_settings);
		}

		/// <summary>
		/// A route constraint for the attachments route, that ignores any controller or action route requests so only the
		/// /attachments/{filename} routes get through.
		/// </summary>
		public class IgnoreMvcConstraint : IRouteConstraint
		{
			private ApplicationSettings _settings;

			public IgnoreMvcConstraint(ApplicationSettings settings)
			{
				_settings = settings;
			}

			public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
			{
				if (routeDirection == RouteDirection.UrlGeneration)
					return false;
				if (values.ContainsKey("controller") || values.ContainsKey("action"))
					return false;

				if (route.Url.StartsWith(_settings.AttachmentsRoutePath +"/{*filename}"))
					return true;
				else
					return false;
			}
		}
	}
}
