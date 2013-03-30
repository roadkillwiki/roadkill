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
			// http://codebetter.com/jeremymiller/2011/01/23/if-you-are-using-structuremap-with-mvc3-please-read-this/
			if (serviceType.IsAbstract || serviceType.IsInterface)
			{
				var x = ObjectFactory.TryGetInstance(serviceType);
				return x;
			}
			else
			{
				var x = ObjectFactory.GetInstance(serviceType);
				return x;
			}
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return ObjectFactory.GetAllInstances(serviceType).Cast<object>();
		}
	}
}
