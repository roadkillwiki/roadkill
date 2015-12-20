using Mindscape.LightSpeed;
using Roadkill.Core.Database;
using StructureMap;
using StructureMap.Web;

namespace Roadkill.Core.DependencyResolution.StructureMap
{
	public class LightSpeedRegistry : Registry
	{
		public LightSpeedRegistry()
		{
			For<LightSpeedContext>().Use("LightSpeedContext", x =>
			{
				var factory = x.TryGetInstance<IRepositoryFactory>() as RepositoryFactory;

				if (factory == null)
					return null;

				return factory.Context;
			});

			For<IUnitOfWork>()
				.HybridHttpOrThreadLocalScoped()
				.Use(x => x.GetInstance<LightSpeedContext>().CreateUnitOfWork());
		}
	}
}