using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Core.Extensions
{
	public static class HtmlHelperExtensions
	{
		public static IHtmlContent DropDownBox(this IHtmlHelper helper, string name, IDictionary<string, string> items, string selectedValue)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (string key in items.Keys)
			{
				SelectListItem selectListItem = new SelectListItem
				{
					Text = items[key],
					Value = key
				};

				if (key == selectedValue)
					selectListItem.Selected = true;

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList);
		}

		public static IHtmlContent DropDownBox(this IHtmlHelper helper, string name, IEnumerable<string> items)
		{
			List<SelectListItem> selectList = new List<SelectListItem>();

			foreach (string item in items)
			{
				SelectListItem selectListItem = new SelectListItem
				{
					Text = item,
					Value = item
				};

				selectList.Add(selectListItem);
			}

			return helper.DropDownList(name, selectList, new { id = name });
		}

		/// <summary>
		/// Renders the HTML of the page with the tag (preferring locked pages), or an empty string if no page has the tag.
		/// </summary>
		public static IHtmlContent RenderPageByTag(this IHtmlHelper helper, string tag)
		{
			string html = "";

			IPageService pageService = helper.ViewContext.HttpContext.RequestServices.GetService<IPageService>();
			if (pageService != null)
			{
				IEnumerable<PageViewModel> pages = pageService.FindByTag(tag);
				if (pages.Any())
				{
					// Find the page, first search for a locked page.
					PageViewModel model = pages.FirstOrDefault(h => h.IsLocked) ?? pages.FirstOrDefault();

					if (model != null)
						html = model.ContentAsHtml;
				}
			}

			return new HtmlString(html);
		}

		public static IHtmlContent DialogPartial(this IHtmlHelper helper, string viewName)
		{
			// Searches the controller's Dialogs folder, then Views/Shared/Dialogs (as with ASP.NET MVC 5).
			return helper.Partial("Dialogs/" + viewName);
		}

		public static IHtmlContent DialogPartial(this IHtmlHelper helper, string viewName, object model)
		{
			return helper.Partial(viewName, model);
		}

		public static IHtmlContent SiteSettingsNavigation(this IHtmlHelper htmlHelper)
		{
			return htmlHelper.Partial("Navigation");
		}
	}
}
