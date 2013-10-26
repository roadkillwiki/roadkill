using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers.Api
{
	public class PagesController : ApiControllerBase
	{
		private IPageService _pageService;

		public PagesController(IPageService pageService)
		{
			_pageService = pageService;
		}

		/// <summary>
		/// Retrieves all pages from the system, but without their text content.
		/// </summary>
		/// <returns></returns>
		[WebApiAdminRequired]
		public IEnumerable<PageViewModel> Get()
		{
			return _pageService.AllPages();
		}

		/// <summary>
		/// Retrieves a page by its id.
		/// </summary>
		/// <param name="id">The id of the page.</param>
		/// <returns></returns>
		[WebApiAdminRequired]
		public PageViewModel Get(int id)
		{
			return _pageService.GetById(id);
		}

		[WebApiAdminRequired]
		public void Put(PageViewModel model)
		{
			_pageService.UpdatePage(model);
		}
	}
}
