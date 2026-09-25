using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Exports the ModelState errors to TempData when redirecting (post-redirect-get).
	/// </summary>
	public class ExportModelStateAttribute : ActionFilterAttribute
	{
		internal static readonly string Key = "MODELSTATE_TEMPDATA";

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			// Based on: http://weblogs.asp.net/rashid/archive/2009/04/01/asp-net-mvc-best-practices-part-1.aspx#prg
			Controller controller = filterContext.Controller as Controller;

			// Only export when ModelState is not valid, and we are redirecting
			if (controller != null && !controller.ModelState.IsValid && IsRedirect(filterContext.Result))
			{
				// TempData must be serializable, so only the errors are stored (key -> error messages).
				Dictionary<string, string[]> errors = controller.ModelState
					.Where(x => x.Value.Errors.Count > 0)
					.ToDictionary(x => x.Key, x => x.Value.Errors.Select(e => e.ErrorMessage).ToArray());

				controller.TempData[Key] = System.Text.Json.JsonSerializer.Serialize(errors);
			}

			base.OnActionExecuted(filterContext);
		}

		private static bool IsRedirect(IActionResult result)
		{
			return result is RedirectResult || result is RedirectToRouteResult || result is RedirectToActionResult || result is LocalRedirectResult;
		}
	}
}
