using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http.Dependencies;
using Microsoft.Practices.ServiceLocation;
using StructureMap;
using StructureMap.Pipeline;
using IDependencyResolver = System.Web.Mvc.IDependencyResolver;

namespace Roadkill.Core.DependencyResolution.StructureMap
{
	// This is boiler plate code adapted from the StructureMapDependencyScope class from Structuremap.MVC
	public class StructureMapServiceLocator : ServiceLocatorImplBase, IDependencyResolver, System.Web.Http.Dependencies.IDependencyResolver
	{
		private const string NestedContainerKey = "Nested.Container.Key";
		public IContainer Container { get; set; }
		public bool IsWeb { get; set; }

		[ThreadStatic]
		private static IContainer _container;

		public IContainer CurrentNestedContainer
		{
			get
			{
				if (IsWeb)
				{
					return (IContainer) HttpContext.Items[NestedContainerKey];
				}
				else
				{
					return _container;
				}
			}
			set
			{
				if (IsWeb)
				{
					HttpContext.Items[NestedContainerKey] = value;
				}
				else
				{
					_container = value;
				}
			}
		}

		private HttpContextBase HttpContext
		{
			get
			{
				var ctx = Container.TryGetInstance<HttpContextBase>();
				return ctx ?? new HttpContextWrapper(System.Web.HttpContext.Current);
			}
		}

		public StructureMapServiceLocator(IContainer container, bool isWeb)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}

			IsWeb = isWeb;
			Container = container;

			if (!IsWeb)
			{
				CreateNestedContainer();
			}
		}

		public void CreateNestedContainer()
		{
			if (CurrentNestedContainer != null)
			{
				return;
			}
			CurrentNestedContainer = Container.GetNestedContainer();
		}

		public void Dispose()
		{
			DisposeNestedContainer();
			Container.Dispose();
		}

		public void DisposeNestedContainer()
		{
			if (CurrentNestedContainer != null)
			{
				CurrentNestedContainer.Dispose();
				CurrentNestedContainer = null;
			}
		}

		public void RegisterType<T>(T instance)
		{
			CurrentNestedContainer.Configure(x => x.For<T>().AddInstance(new ObjectInstance(instance)));
		}

		public IEnumerable<object> GetServices(Type serviceType)
		{
			return DoGetAllInstances(serviceType);
		}

		protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
		{
			return (CurrentNestedContainer ?? Container).GetAllInstances(serviceType).Cast<object>();
		}

		protected override object DoGetInstance(Type serviceType, string key)
		{
			IContainer container = (CurrentNestedContainer ?? Container);

			if (string.IsNullOrEmpty(key))
			{
				return serviceType.IsAbstract || serviceType.IsInterface
					? container.TryGetInstance(serviceType)
					: container.GetInstance(serviceType);
			}

			return container.GetInstance(serviceType, key);
		}

		#region WebApi IDependencyResolver
		public IDependencyScope BeginScope()
		{
			IContainer child = Container.GetNestedContainer();
			return new StructureMapServiceLocator(child, true);
		}
		#endregion
	}
}
