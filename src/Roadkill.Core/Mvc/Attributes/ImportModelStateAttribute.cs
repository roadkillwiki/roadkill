using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Represents an attribute that is used to import a ModelState from an action that has performed a RedirectToAction().
	/// That action should be decorated with the <see cref="ExportModelStateAttribute"/>.
	/// </summary>
	public class ImportModelStateAttribute : ActionFilterAttribute
	{
		protected static readonly string _key = "MODELSTATE_TEMPDATA";

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			// Based on: http://weblogs.asp.net/rashid/archive/2009/04/01/asp-net-mvc-best-practices-part-1.aspx#prg
			ModelStateDictionary modelState = filterContext.Controller.TempData[_key] as ModelStateDictionary;

			if (modelState != null)
			{
				// Only Import if we are viewing
				if (filterContext.Result is ViewResult)
				{
					filterContext.Controller.ViewData.ModelState.Merge(modelState);
				}
				else
				{
					// Otherwise remove it.
					filterContext.Controller.TempData.Remove(_key);
				}
			}

			base.OnActionExecuted(filterContext);
		}
	}
}
