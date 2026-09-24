using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Imports the ModelState errors exported by the <see cref="ExportModelStateAttribute"/>.
	/// </summary>
	public class ImportModelStateAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			// Based on: http://weblogs.asp.net/rashid/archive/2009/04/01/asp-net-mvc-best-practices-part-1.aspx#prg
			Controller controller = filterContext.Controller as Controller;
			string json = controller?.TempData[ExportModelStateAttribute.Key] as string;

			if (json != null)
			{
				// Only Import if we are viewing
				if (filterContext.Result is ViewResult)
				{
					var errors = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string[]>>(json);
					foreach (KeyValuePair<string, string[]> error in errors)
					{
						foreach (string message in error.Value)
						{
							controller.ModelState.AddModelError(error.Key, message);
						}
					}
				}
				else
				{
					// Otherwise remove it.
					controller.TempData.Remove(ExportModelStateAttribute.Key);
				}
			}

			base.OnActionExecuted(filterContext);
		}
	}
}
