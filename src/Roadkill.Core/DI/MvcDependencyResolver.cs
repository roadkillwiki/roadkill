using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using StructureMap;

namespace Roadkill.Core
{
	public class MvcDependencyResolver : IDependencyResolver
	{
		public object GetService(Type serviceType)
		{
			return DependencyContainer.GetInstance(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return DependencyContainer.GetAllInstances(serviceType);
		}
	}
}
