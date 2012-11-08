using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web;

namespace Roadkill.Core.Files
{
	/// <summary>
	/// A route handler for the attachments virtual folder, which doesn't swallow MVC routes.
	/// </summary>
	public class AttachmentRouteHandler : IRouteHandler
	{
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{
			return new AttachmentFileHandler();
		}

		/// <summary>
		/// Registers the /Attachments/ path (the name is taken from a web.config setting)
		/// </summary>
		public static void Register()
		{
			Route route = new Route(RoadkillSettings.Current.AttachmentsRoutePath + "/{*filename}", new AttachmentRouteHandler());
			route.Constraints = new RouteValueDictionary();
			route.Constraints.Add("MvcContraint", new IgnoreMvcConstraint());

			RouteTable.Routes.Add(route);
		}

		internal class IgnoreMvcConstraint : IRouteConstraint
		{
			public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
			{
				if (routeDirection == RouteDirection.UrlGeneration)
					return false;
				if (values.ContainsKey("controller") || values.ContainsKey("action"))
					return false;

				// Remove the starting "/" for the route table
				if (route.Url.StartsWith(RoadkillSettings.Current.AttachmentsRoutePath + "/"))
					return true;
				else
					return false;
			}
		}
	}
}
