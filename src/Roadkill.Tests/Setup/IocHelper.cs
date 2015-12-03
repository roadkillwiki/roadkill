using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Tests.Unit;
using Roadkill.Tests.Unit.StubsAndMocks;

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
			LocatorStartup.StartMVCInternal(registry, false);

			LocatorStartup.Locator.Container.Configure(x =>
			{
				if (stubRepository)
					x.For<IRepository>().Use(new RepositoryMock());

				x.For<IUserContext>().Use(new UserContextStub());
			});

			//LocatorStartup.AfterInitialization(LocatorStartup.Locator.Container, LocatorStartup.Locator.Container.GetInstance<ApplicationSettings>());
		}

		public static void ClearIoC()
		{
			LocatorStartup.Locator.DisposeNestedContainer();
		}
	}
}
