using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StructureMap;

namespace Roadkill.Core.DI
{
	public class ServiceLocator
	{
		/// <summary>
		/// Gets the current instance of T from the IoC, or returns null if doesn't exist.
		/// </summary>
		public static T GetInstance<T>()
		{
			return ObjectFactory.TryGetInstance<T>();
		}

		public static object GetInstance(Type instanceType)
		{
			// This workaround is from http://codebetter.com/jeremymiller/2011/01/23/if-you-are-using-structuremap-with-mvc3-please-read-this/
			if (instanceType.IsAbstract || instanceType.IsInterface)
			{
				var x = ObjectFactory.TryGetInstance(instanceType);
				return x;
			}
			else
			{
				var x = ObjectFactory.GetInstance(instanceType);
				return x;
			}
		}

		public static IEnumerable<object> GetAllInstances(Type instanceType)
		{
			return ObjectFactory.GetAllInstances(instanceType).Cast<object>();
		}

		public static IEnumerable<T> GetAllInstances<T>()
		{
			return ObjectFactory.GetAllInstances<T>();
		}

		public static void RegisterType<T>(T instance)
		{
			ObjectFactory.Container.Configure(x => x.Register<T>(instance));
		}
	}
}
