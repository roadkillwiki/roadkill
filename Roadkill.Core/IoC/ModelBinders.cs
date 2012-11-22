using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using StructureMap;

namespace Roadkill.Core
{
	public class UserSummaryModelBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			return ObjectFactory.GetInstance<UserSummary>();
		}
	}

	public class SettingsSummaryModelBinder : IModelBinder
	{
		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
		{
			return ObjectFactory.GetInstance<SettingsSummary>();
		}
	}
}
