using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Roadkill.Core.Logging;
using StructureMap;

namespace Roadkill.Core.DI.Mvc
{
	/// <summary>
	/// The factory for all MVC attributes. This should be marked internal to avoid Structuremap issues.
	/// </summary>
	internal class MvcAttributeProvider : FilterAttributeFilterProvider, System.Web.Http.Filters.IFilterProvider
	{
		IEnumerable<System.Web.Http.Filters.IFilterProvider> _webApiProviders;

		public MvcAttributeProvider()
		{
		}

		// For web api
		public MvcAttributeProvider(IEnumerable<System.Web.Http.Filters.IFilterProvider> providers)
		{
			_webApiProviders = providers;
		}

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
				// Injects the instance with Structuremap's dependencies
				Log.Information(filter.Instance.GetType().Name);
				ObjectFactory.BuildUp(filter.Instance);
			}

			return filters;
		}

		// WebApi

		public IEnumerable<System.Web.Http.Filters.FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
		{
			if (_webApiProviders != null)
			{
				IEnumerable<System.Web.Http.Filters.IFilterProvider> filterProviders = _webApiProviders;
				IEnumerable<System.Web.Http.Filters.FilterInfo> filters = filterProviders.SelectMany(x => x.GetFilters(configuration, actionDescriptor)).ToList();

				foreach (System.Web.Http.Filters.FilterInfo filter in filters)
				{
					// Injects the instance with Structuremap's dependencies
					Log.Information(filter.Instance.GetType().Name);
					ObjectFactory.BuildUp(filter.Instance);
				}

				return filters;
			}
			else
			{
				// _webApiProviders will be null for something
				return null;
			}
		}
	}
}
