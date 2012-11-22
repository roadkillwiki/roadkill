using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using StructureMap;

namespace Roadkill.Core.Configuration
{
	public class StructureMapControllerFactory : DefaultControllerFactory
	{
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
