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
	/// A route handler for the attachments virtual folder, which doesn't swallow MVC routes.
	/// </summary>
	public class AttachmentRouteHandler : IRouteHandler
	{
		private ApplicationSettings _settings;

		public AttachmentRouteHandler(ApplicationSettings settings)
		{
			_settings = settings;
		}

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

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new AttachmentFileHandler(_settings);
		}

		internal class IgnoreMvcConstraint : IRouteConstraint
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
