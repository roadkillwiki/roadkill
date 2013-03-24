using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Roadkill.Core.Controllers;
using StructureMap;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// The factory for all Controller instances in Roadkill, used by the MVC framework.
	/// </summary>
	public class ControllerFactory : DefaultControllerFactory
	{
		/// <summary>
		/// Creates an instance of the controller given by the controllerType argument. This uses 
		/// StructureMap's registered types to create the instance via the ObjectFactory.
		/// </summary>
		/// <param name="requestContext">The current context</param>
		/// <param name="controllerType">The type of the controller.</param>
		/// <returns>The controller instance.</returns>
		protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
		{
			try
			{
				if (requestContext == null || controllerType == null)
				{
					return base.GetControllerInstance(requestContext, controllerType);
				}

				Controller controller = ObjectFactory.GetInstance(controllerType) as Controller;
				if (controller != null)
				{
					return controller;
				}
				else
				{
					return base.GetControllerInstance(requestContext, controllerType);
				}
			}
			catch (StructureMapException e)
			{
				Log.Error("An error occured with the ControllerFactory: {0}", e);

				if (requestContext.HttpContext.IsCustomErrorEnabled)
					return new ErrorController();
				else
					throw e;
			}
		}
	}

	public class ErrorController : Controller
	{
		public ActionResult Index()
		{
			return Content("Your Roadkill installation has unrecoverable errors. View your log file or turn customErrors off to view the errors.");
		}
	}
}
