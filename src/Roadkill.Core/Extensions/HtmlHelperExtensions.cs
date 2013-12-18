using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Roadkill.Core.Converters;
using System.Web;
using System.Text.RegularExpressions;
using Recaptcha;
using System.Web.UI;
using System.IO;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using StructureMap;
using ControllerBase = Roadkill.Core.Mvc.Controllers.ControllerBase;
using Roadkill.Core.Attachments;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Localization;
using Roadkill.Core.DI;
using System.Linq.Expressions;
using Roadkill.Core.Mvc.Controllers;

namespace Roadkill.Core.Extensions
{
	/// <summary>
	/// Roadkill specific extensions methods for the <see cref="HtmlHelper"/> class.
	/// </summary>
	public static class HtmlHelperExtensions
	{
		/// <summary>
		/// Creates a drop down list from an <c>IDictionary</c> and selects the item.
		/// </summary>
		public static MvcHtmlString DropDownBox(this HtmlHelper helper, string name, IDictionary<string, string> items, string selectedValue)
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

		/// <summary>
		/// Creates a drop down list from an <c>IList</c> of strings.
		/// </summary>
		public static MvcHtmlString DropDownBox(this HtmlHelper helper, string name, IEnumerable<string> items)
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
        /// Render the first page which has this tag. Admin locked pages have priority. 
        /// </summary>
        /// <param name="tag">the tagname</param>
        /// <returns>html</returns>
        /// <example>
        /// usage:   @Html.RenderPageByTag("secondMenu")
        /// </example>
        public static MvcHtmlString RenderPageByTag(this HtmlHelper helper, string tag)
        {
			string html = "";

			ControllerBase controller = helper.ViewContext.Controller as ControllerBase;
			WikiController wikiController = controller as WikiController;
			if (wikiController != null)
			{
				PageService pageService = wikiController.PageService;

				IEnumerable<PageViewModel> pages = pageService.FindByTag(tag);
				if (pages.Count() > 0)
				{
					// Find the page, first search for a locked page.
					PageViewModel model = pages.FirstOrDefault(h => h.IsLocked);
					if (model == null)
					{
						model = pages.FirstOrDefault();
					}

					if (model != null)
					{
						html = model.ContentAsHtml;
					}
				}
			}

			return MvcHtmlString.Create(html);
        }

		/// <summary>
		/// An alias for Partial() to indicate a dialog's HTML is being rendered.
		/// </summary>
		public static MvcHtmlString DialogPartial(this HtmlHelper helper, string viewName)
		{
			return helper.Partial("Dialogs/" + viewName);
		}

		/// <summary>
		/// An alias for Partial() to indicate a dialog's HTML is being rendered.
		/// </summary>
		public static MvcHtmlString DialogPartial(this HtmlHelper helper, string viewName, object model)
		{
			return helper.Partial(viewName, model);
		}

		/// <summary>
		/// Returns the rendered partial navigation menu.
		/// </summary>
		public static MvcHtmlString SiteSettingsNavigation(this HtmlHelper htmlHelper)
		{
			return htmlHelper.Partial("Navigation");
		}
	}
}
