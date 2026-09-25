using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Roadkill.Core.Mvc
{
	/// <summary>
	/// Provides access to the current <see cref="HttpContext"/> for the (few) places that previously used
	/// System.Web.HttpContext.Current. The accessor is set on application startup.
	/// </summary>
	public static class HttpContextHolder
	{
		/// <summary>
		/// The accessor registered by the web application, or null when running outside of a web request (e.g. tests).
		/// </summary>
		public static IHttpContextAccessor Accessor { get; set; }

		/// <summary>
		/// The current HttpContext, or null if there isn't a request.
		/// </summary>
		public static HttpContext Current
		{
			get { return Accessor?.HttpContext; }
		}

		/// <summary>
		/// Converts a virtual path (starting with "~/") into an application absolute path, including any path base
		/// the application is running under, e.g. "~/Plugins/Toc" becomes "/wiki/Plugins/Toc".
		/// </summary>
		public static string ResolveUrl(string virtualPath)
		{
			if (string.IsNullOrEmpty(virtualPath) || !virtualPath.StartsWith("~"))
				return virtualPath;

			string pathBase = Current?.Request.PathBase.Value ?? "";
			string path = virtualPath.Substring(1);
			if (!path.StartsWith("/"))
				path = "/" + path;

			return pathBase.TrimEnd('/') + path;
		}
	
		/// <summary>
		/// Generates a url for the controller action using the current request (the equivalent of the MVC5 UrlHelper.Action
		/// method), or returns null if there is no current request.
		/// </summary>
		public static string Action(string action, string controller, object values = null, string area = "")
		{
			HttpContext context = Current;
			if (context == null)
				return null;

			LinkGenerator linkGenerator = context.RequestServices.GetService<LinkGenerator>();
			if (linkGenerator == null)
				return null;

			RouteValueDictionary routeValues = new RouteValueDictionary(values);
			routeValues["area"] = area ?? "";

			return linkGenerator.GetPathByAction(context, action, controller, routeValues);
		}
	}
}
