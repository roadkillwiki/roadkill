using System;
using Microsoft.AspNetCore.Http;
using Roadkill.Core.Mvc;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Text
{
	public class UrlResolver
	{
		private readonly HttpContext _httpContext;

		public UrlResolver(HttpContext httpContext = null)
		{
			_httpContext = httpContext;
		}

		/// <summary>
		/// Converts relative paths to absolute ones, e.g. ~/mydir/page1.html to /mywiki/mydir/page1.html.
		/// </summary>
		/// <returns>An absolute path for the resource.</returns>
		public virtual string ConvertToAbsolutePath(string relativeUrl)
		{
			if (_httpContext != null)
				return HttpContextHolder.ResolveUrl(relativeUrl);

			return relativeUrl;
		}

		/// <summary>
		/// Gets the internal url of a page based on the page title.
		/// </summary>
		/// <param name="id">The page id</param>
		/// <param name="title">The title of the page</param>
		/// <returns>An absolute path to the page.</returns>
		public virtual string GetInternalUrlForTitle(int id, string title)
		{
			string url = null;
			if (_httpContext != null)
				url = HttpContextHolder.Action("Index", "Wiki", new { id = id, title = PageViewModel.EncodePageTitle(title) });

			// The fallback is really here for tests
			return url ?? string.Format("/wiki/{0}/{1}", id, PageViewModel.EncodePageTitle(title));
		}

		/// <summary>
		/// Gets a url to the new page resource, appending the title to the querystring.
		/// For example /pages/new?title=xyz
		/// </summary>
		public virtual string GetNewPageUrlForTitle(string title)
		{
			string url = null;
			if (_httpContext != null)
				url = HttpContextHolder.Action("New", "Pages", new { title = title });

			return url ?? string.Format("/pages/new/?title={0}", title);
		}
	}
}
