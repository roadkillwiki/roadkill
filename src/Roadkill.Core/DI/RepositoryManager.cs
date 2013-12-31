using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using StructureMap;

namespace Roadkill.Core.DI
{
	public class RepositoryManager
	{
		public static IRepository LoadRepositoryFromType(string typeName)
		{
			Type repositoryType = typeof(IRepository);
			Type reflectedType = Type.GetType(typeName);

			if (repositoryType.IsAssignableFrom(reflectedType))
			{
				return (IRepository)ObjectFactory.GetInstance(reflectedType);
			}
			else
			{
				throw new IoCException(null, "The type {0} specified in the repositoryType web.config setting is not an instance of a IRepository.", typeName);
			}
		}

		public static IRepository ChangeRepository(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			if (dataStoreType.RequiresCustomRepository)
			{
				IRepository customRepository = LoadRepositoryFromType(dataStoreType.CustomRepositoryType);
				ObjectFactory.Configure(x =>
				{
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(customRepository);
				});
			}
			else
			{
				ObjectFactory.Configure(x =>
				{
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use<LightSpeedRepository>();
				});
			}

			IRepository repository = ObjectFactory.GetInstance<IRepository>();
			repository.Startup(dataStoreType, connectionString, enableCache);
			return repository;
		}

		public static void DisposeRepository()
		{
			ApplicationSettings settings = ObjectFactory.GetInstance<ApplicationSettings>();

			// Don't try to dispose a repository if the app isn't installed, as the repository won't be correctly configured.
			// (as no connection string is set, the Startup doesn't complete and the IUnitOfWork isn't registered with StructureMap)
			if (settings.Installed)
			{
				IRepository repository = ObjectFactory.GetInstance<IRepository>();

				if (settings.Installed)
				{
					ObjectFactory.GetInstance<IRepository>().Dispose();
				}
			}
		}

		/// <summary>
		/// Tests the database connection, changing the current registered repository.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <returns>Any error messages or an empty string if no errors occurred.</returns>
		public static string TestDbConnection(string connectionString, string databaseType)
		{
			try
			{
				DataStoreType dataStoreType = DataStoreType.ByName(databaseType);
				if (dataStoreType == null)
					dataStoreType = DataStoreType.ByName("SQLServer2005");

				IRepository repository = ChangeRepository(dataStoreType, connectionString, false);
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
				ApplicationSettings appSettings = ObjectFactory.GetInstance<ApplicationSettings>();
				IRepository repository = ChangeRepository(appSettings.DataStoreType, appSettings.ConnectionString, false);
			}
		}
	}
}
