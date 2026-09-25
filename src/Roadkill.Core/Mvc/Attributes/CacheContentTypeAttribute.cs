using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Sets the content type of the response, e.g. for the javascript variables views.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class CacheContentTypeAttribute : Attribute, IResultFilter
	{
		public string ContentType { get; set; }

		/// <summary>
		/// The cache duration in seconds (sent as a Cache-Control header).
		/// </summary>
		public int Duration { get; set; }

		/// <summary>
		/// Not used - kept for compatibility with the ASP.NET MVC OutputCache attribute arguments.
		/// </summary>
		public string VaryByParam { get; set; }

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			filterContext.HttpContext.Response.ContentType = ContentType ?? "text/html";

			if (Duration > 0)
				filterContext.HttpContext.Response.Headers["Cache-Control"] = "public, max-age=" + Duration;
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}
	}
}
