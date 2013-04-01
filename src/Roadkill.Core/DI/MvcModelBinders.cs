using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Mvc.ViewModels;
using StructureMap;

namespace Roadkill.Core
{
	/// <summary>
	/// Used by the MVC framework to create all instances of a <see cref="UserSummary"/> view model object.
	/// </summary>
	public class UserSummaryModelBinder : DefaultModelBinder
	{
		protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
		{
			return ObjectFactory.GetInstance<UserSummary>();
		}
	}

	/// <summary>
	/// Used by the MVC framework to create all instances of a <see cref="SettingsSummary"/> view model object.
	/// </summary>
	public class SettingsSummaryModelBinder : DefaultModelBinder
	{
		protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
		{
			return ObjectFactory.GetInstance<SettingsSummary>();
		}
	}
}
