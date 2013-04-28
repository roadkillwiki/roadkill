using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Logging;
using StructureMap;

namespace Roadkill.Core
{
	public class MvcAttributeProvider : FilterAttributeFilterProvider
	{
		protected override IEnumerable<FilterAttribute> GetControllerAttributes(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
		{
			IEnumerable<FilterAttribute> filters = base.GetControllerAttributes(controllerContext, actionDescriptor);

			foreach (FilterAttribute filter in filters)
			{
				ObjectFactory.BuildUp(filter);
			}

			return filters;
		}

		protected override IEnumerable<FilterAttribute> GetActionAttributes(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
		{
			IEnumerable<FilterAttribute> filters = base.GetActionAttributes(controllerContext, actionDescriptor);

			foreach (FilterAttribute filter in filters)
			{
				ObjectFactory.BuildUp(filter);
			}

			return filters;
		}

		public IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
		{
			IEnumerable<Filter> filters = base.GetFilters(controllerContext, actionDescriptor);

			foreach (Filter filter in filters)
			{
				Log.Information(filter.Instance.GetType().Name);

				ObjectFactory.BuildUp(filter.Instance);
			}

			return filters;
		}
	}
}
