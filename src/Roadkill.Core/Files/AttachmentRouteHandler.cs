using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Files
{
	/// <summary>
	/// A route handler for the attachments virtual folder, which doesn't swallow MVC routes.
	/// </summary>
	public class AttachmentRouteHandler : IRouteHandler
	{
		private IConfigurationContainer _config;

		public AttachmentRouteHandler(IConfigurationContainer config)
		{
			Route route = new Route(config.ApplicationSettings.AttachmentsRoutePath + "/{*filename}", new AttachmentRouteHandler(config));
			route.Constraints = new RouteValueDictionary();
			route.Constraints.Add("MvcContraint", new IgnoreMvcConstraint(config));

			RouteTable.Routes.Add(route);
			_config = config;
		}

		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new AttachmentFileHandler(_config);
		}

		internal class IgnoreMvcConstraint : IRouteConstraint
		{
			private IConfigurationContainer _config;

			public IgnoreMvcConstraint(IConfigurationContainer config)
			{
				_config = config;
			}

			public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
			{
				if (routeDirection == RouteDirection.UrlGeneration)
					return false;
				if (values.ContainsKey("controller") || values.ContainsKey("action"))
					return false;

				// Remove the starting "/" for the route table
				if (route.Url.StartsWith(_config.ApplicationSettings.AttachmentsRoutePath + "/"))
					return true;
				else
					return false;
			}
		}
	}
}
