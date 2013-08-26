using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.DI;
using StructureMap;

namespace Roadkill.Core
{
	public class MvcDependencyResolver : IDependencyResolver
	{
		public object GetService(Type serviceType)
		{
			return ServiceLocator.GetInstance(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return ServiceLocator.GetAllInstances(serviceType);
		}
	}
}
