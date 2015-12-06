using System;
using System.Linq;
using System.Web.Mvc;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Tests.Unit;
using Roadkill.Tests.Unit.StubsAndMocks;
using StructureMap;

namespace Roadkill.Tests.Setup
{
	public class IocHelper
	{
		public static void ConfigureLocator(ApplicationSettings settings = null, bool stubRepository = true)
		{
			if (settings == null)
				settings = new ApplicationSettings();

			var configReader = new ConfigReaderWriterStub();
			configReader.ApplicationSettings = settings;

			var registry = new RoadkillRegistry(configReader);
			var container = new Container(registry);
			container.Configure(x =>
			{
				if (stubRepository)
				{
					x.Scan(a => a.AssemblyContainingType<IocHelper>());
					x.For<IRepository>().Use(new RepositoryMock());
				}

				x.For<IUserContext>().Use(new UserContextStub());
			});

			LocatorStartup.Locator = new StructureMapServiceLocator(container, false);
			DependencyResolver.SetResolver(LocatorStartup.Locator);

			var all = container.Model.AllInstances.OrderBy(t => t.PluginType.Name).Select(t => string.Format("{0}:{1}", t.PluginType.Name, t.ReturnedType.AssemblyQualifiedName));
            Console.WriteLine(string.Join("\n", all));
		}

		public static void ClearIoC()
		{
			LocatorStartup.Locator.DisposeNestedContainer();
		}
	}
}
