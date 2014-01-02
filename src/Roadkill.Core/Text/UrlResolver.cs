using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Roadkill.Core.Extensions;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Text
{
	public class UrlResolver
	{
		private HttpContextBase _httpContext;

		public UrlResolver(HttpContextBase httpContext = null)
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
			{
				UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				return helper.Content(relativeUrl);
			}
			else
			{
				return relativeUrl;
			}
		}

		/// <summary>
		/// Gets the internal url of a page based on the page title.
		/// </summary>
		/// <param name="id">The page id</param>
		/// <param name="title">The title of the page</param>
		/// <returns>An absolute path to the page.</returns>
		public virtual string GetInternalUrlForTitle(int id, string title)
		{
			if (_httpContext != null)
			{
				UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				return helper.Action("Index", "Wiki", new { id = id, title = PageViewModel.EncodePageTitle(title) });
			}
			else
			{
				// This is really here as a fallback, for tests
				return string.Format("/wiki/{0}/{1}", id, PageViewModel.EncodePageTitle(title));
			}
		}

		/// <summary>
		/// Gets a url to the new page resource, appending the title to the querystring.
		/// For example /pages/new?title=xyz
		/// </summary>
		/// <param name="title"></param>
		/// <returns></returns>
		public virtual string GetNewPageUrlForTitle(string title)
		{
			if (_httpContext != null)
			{
				UrlHelper helper = new UrlHelper(HttpContext.Current.Request.RequestContext);
				return helper.Action("New", "Pages", new { title = title });
			}
			else
			{
				return string.Format("/pages/new/?title={0}", title);
			}
		}
	}
}
