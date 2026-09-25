using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.WebApi
{
	/// <summary>
	/// REST api for pages: GET api/pages, GET api/pages/{id}, POST api/pages, PUT api/pages
	/// </summary>
	[ApiController]
	[Route("api/pages")]
	[ApiKeyAuthorize]
	public class PagesController : Microsoft.AspNetCore.Mvc.ControllerBase
	{
		private readonly IPageService _pageService;

		public PagesController(IPageService pageService)
		{
			_pageService = pageService;
		}

		[HttpGet]
		public IEnumerable<PageViewModel> Get()
		{
			return _pageService.AllPages();
		}

		[HttpGet("{id:int}")]
		public PageViewModel Get(int id)
		{
			return _pageService.GetById(id);
		}

		[HttpPost]
		public void Post(PageViewModel model)
		{
			_pageService.AddPage(model);
		}

		[HttpPut]
		public void Put(PageViewModel model)
		{
			_pageService.UpdatePage(model);
		}
	}
}
