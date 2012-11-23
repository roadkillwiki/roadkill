using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using StructureMap;

namespace Roadkill.Core
{
	public class UserSummaryModelBinder : DefaultModelBinder
	{
		protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
		{
			return base.CreateModel(controllerContext, bindingContext, modelType);
		}
	}

	public class SettingsSummaryModelBinder : DefaultModelBinder
	{
		protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
		{
			return ObjectFactory.GetInstance<SettingsSummary>();
		}
	}
}
