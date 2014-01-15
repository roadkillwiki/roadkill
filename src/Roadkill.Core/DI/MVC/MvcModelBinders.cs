using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Mvc.ViewModels;
using StructureMap;

namespace Roadkill.Core.DI.Mvc
{
	/// <summary>
	/// Used by the MVC framework to create all instances of a <see cref="UserViewModel"/> view model object.
	/// </summary>
	internal class UserViewModelModelBinder : DefaultModelBinder
	{
		protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
		{
			return ObjectFactory.GetInstance<UserViewModel>();
		}
	}

	/// <summary>
	/// Used by the MVC framework to create all instances of a <see cref="SettingsViewModel"/> view model object.
	/// </summary>
	internal class SettingsViewModelBinder : DefaultModelBinder
	{
		protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
		{
			return ObjectFactory.GetInstance<SettingsViewModel>();
		}
	}
}
