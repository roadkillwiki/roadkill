using System;
using System.Linq;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.DependencyResolution.StructureMap;
using StructureMap.Web;

namespace Roadkill.Core.DependencyResolution
{
	public class RepositoryManager
	{
		private readonly StructureMapServiceLocator _serviceLocator;

		public RepositoryManager()
		{
			_serviceLocator = LocatorStartup.Locator;
		}

		/// TODO: Tests
		public IRepository ChangeRepository(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			if (dataStoreType.RequiresCustomRepository)
			{
				string typeName = dataStoreType.CustomRepositoryType;

				_serviceLocator.Container.Configure(x =>
				{
					x.For<IRepository>()
						.HybridHttpOrThreadLocalScoped()
						.Use(context => context.All<IRepository>().First(t => t.GetType().Name == typeName));
				});
			}
			else
			{
				_serviceLocator.Container.Configure(x =>
				{
					x.For<IRepository>()
						.HybridHttpOrThreadLocalScoped()
						.Use<LightSpeedRepository>();
				});
			}

			IRepository repository = _serviceLocator.GetInstance<IRepository>();
			repository.Startup(dataStoreType, connectionString, enableCache);
			return repository;
		}

		/// TODO: Tests
		/// <summary>
		/// Tests the database connection, changing the current registered repository.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <returns>Any error messages or an empty string if no errors occurred.</returns>
		public string TestDbConnection(string connectionString, string databaseType)
		{
			try
			{
				DataStoreType dataStoreType = DataStoreType.ByName(databaseType);
				if (dataStoreType == null)
					dataStoreType = DataStoreType.ByName("SQLServer2005");

				IRepository repository;
				if (dataStoreType.RequiresCustomRepository)
				{
					repository = (IRepository) _serviceLocator.Container.GetInstance(Type.GetType(dataStoreType.Name));
				}
				else
				{
					repository = _serviceLocator.GetInstance<LightSpeedRepository>();
				}

				repository.TestConnection(dataStoreType, connectionString);
				return "";
			}
			catch (Exception e)
			{
				return e.ToString();
			}
			finally
			{
				// Restore to their previous state
				//ApplicationSettings appSettings = _serviceLocator.GetInstance<ApplicationSettings>();
				//ChangeRepository(appSettings.DataStoreType, appSettings.ConnectionString, false);
			}
		}
	}
}
