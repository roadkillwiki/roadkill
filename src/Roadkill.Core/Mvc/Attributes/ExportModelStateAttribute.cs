using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Roadkill.Core.Mvc.Attributes
{
	/// <summary>
	/// Represents an attribute that is used to maintain the ModelState when performing a RedirectToAction().
	/// </summary>
	public class ExportModelStateAttribute : ActionFilterAttribute
	{
		protected static readonly string _key = "MODELSTATE_TEMPDATA";

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			// Based on: http://weblogs.asp.net/rashid/archive/2009/04/01/asp-net-mvc-best-practices-part-1.aspx#prg
			// Only export when ModelState is not valid
			if (!filterContext.Controller.ViewData.ModelState.IsValid)
			{
				// Export if we are redirecting
				if ((filterContext.Result is RedirectResult) || (filterContext.Result is RedirectToRouteResult))
				{
					filterContext.Controller.TempData[_key] = filterContext.Controller.ViewData.ModelState;
				}
			}

			base.OnActionExecuted(filterContext);
		}
	}
}
