using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using StructureMap;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// The factory for all Controller instances in Roadkill, used by the MVC framework.
	/// </summary>
	public class StructureMapControllerFactory : DefaultControllerFactory
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
					return null;

				return (Controller)ObjectFactory.GetInstance(controllerType);
			}
			catch (StructureMapException e)
			{
				throw new IoCException(e,"An error occured with the ControllerFactory: {0}", ObjectFactory.WhatDoIHave());
			}
		}
	}
}
