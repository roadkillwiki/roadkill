using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http.Dependencies;
using System.Web.Mvc;
using Roadkill.Core.DI;
using StructureMap;

namespace Roadkill.Core.DI.Mvc
{
	/// <summary>
	/// Implements both dependency resolvers for MVC and WebApi, to create Structuremap-injected controllers when requested by those frameworks.
	/// </summary>
	internal class MvcDependencyResolver : System.Web.Mvc.IDependencyResolver, System.Web.Http.Dependencies.IDependencyResolver, IDependencyScope
	{
		public object GetService(Type serviceType)
		{
			return ServiceLocator.GetInstance(serviceType);
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return ServiceLocator.GetAllInstances(serviceType);
		}

		public System.Web.Http.Dependencies.IDependencyScope BeginScope()
		{
			return this;
		}

		public void Dispose()
		{
			// Scope is handled by Structuremap
		}
	}
}
