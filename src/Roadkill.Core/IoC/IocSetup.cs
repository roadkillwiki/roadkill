using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Converters;
using StructureMap;
using StructureMap.Graph;

namespace Roadkill.Core
{
	public class IoCSetup
	{
		private IConfigurationContainer _config;
		private IRepository _repository;
		private IRoadkillContext _context;
		private bool _shouldRegisterDefaultInstances;

		public IoCSetup()
		{
			_shouldRegisterDefaultInstances = true;
		}

		public IoCSetup(IConfigurationContainer configurationContainer, IRepository repository, IRoadkillContext context)
		{
			if (configurationContainer == null)
				throw new IoCException("The IConfigurationContainer parameter is null", null);

			if (repository == null)
				throw new IoCException("The IRepository parameter is null", null);

			if (context == null)
				throw new IoCException("The IRoadkillContext parameter is null", null);

			if (configurationContainer.ApplicationSettings == null)
				throw new IoCException("The IConfigurationContainer.ApplicationSettings of the config parameter is null - " + ObjectFactory.WhatDoIHave(), null);

			_config = configurationContainer;
			_repository = repository;
			_context = context;
			_shouldRegisterDefaultInstances = false;
		}

		public void Run()
		{
			//
			// The dependency graph is:
			//
			// - IConfiguration has no dependencies, and (should) lazy load its two properties on demand
			// - IRepository relies on IConfigurationContainer 
			// - RoadkillContext relies on UserManager, which relies on IRepository
			// - Everything else is registered after these 3 types
			//

			ObjectFactory.Initialize(x =>
			{
				RegisterInjectedTypes(x);

				if (_shouldRegisterDefaultInstances)
					RegisterDefaultInstances(x);
				else
					RegisterCustomInstances(x);

				RegisterViewModels(x);
				x.For<IPageManager>().HybridHttpOrThreadLocalScoped().Use<PageManager>();
				RegisterUserManager(x);
			});

			// GetInstance only works once the Configure() method completes
			_repository = ObjectFactory.GetInstance<IRepository>();
			_context = ObjectFactory.GetInstance<IRoadkillContext>();

			// Let the repositories perform any startup tasks (I'm looking at you, NHibernate)
			_repository.Startup(_config.ApplicationSettings.DataStoreType,
								_config.ApplicationSettings.ConnectionString,
								_config.ApplicationSettings.CacheEnabled);
		}

		private void RegisterInjectedTypes(IInitializationExpression x)
		{
			x.Scan(scanner =>
			{
				scanner.AddAllTypesOf<IConfigurationContainer>();
				scanner.AddAllTypesOf<ControllerBase>();
				scanner.AddAllTypesOf<ServiceBase>();
				scanner.AddAllTypesOf<IRoadkillContext>();
				scanner.AddAllTypesOf<IRepository>();
				scanner.AddAllTypesOf<MarkupConverter>();
				scanner.AddAllTypesOf<CustomTokenParser>();
				scanner.WithDefaultConventions();
			});
		}

		private void RegisterDefaultInstances(IInitializationExpression x)
		{
			// Config
			x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped().Use<RoadkillSettings>();
			_config = new RoadkillSettings(); // doesn't have any dependencies to inject, so new'd up is fine.

			// Repository
			if (_config.ApplicationSettings.DataStoreType.RequiresCustomRepository)
			{
				IRepository customRepository = LoadRepositoryFromType(_config.ApplicationSettings.DataStoreType.CustomRepositoryType);
				x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(customRepository);
			}
			else
			{
				x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use<NHibernateRepository>();
			}

			// Context
			x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped().Use<RoadkillContext>();
		}

		private void RegisterCustomInstances(IInitializationExpression x)
		{
			// Config
			x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped().Use(_config);

			// Repository
			x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(_repository);

			// Context
			x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped().Use(_context);
		}

		private void RegisterViewModels(IInitializationExpression x)
		{
			// These models require IConfigurationContainer or IRoadkillContext in their constructors
			x.For<UserSummary>().Use<UserSummary>();
			x.For<SettingsSummary>().Use<SettingsSummary>();
		}

		private void RegisterUserManager(IInitializationExpression x)
		{
			string userManagerTypeName = _config.ApplicationSettings.UserManagerType;
			if (string.IsNullOrEmpty(userManagerTypeName))
			{
				if (_config.ApplicationSettings.UseWindowsAuthentication)
				{
					x.For<IActiveDirectoryService>().HybridHttpOrThreadLocalScoped().Use<DefaultActiveDirectoryService>(); // will the API need this?
					x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use<ActiveDirectoryUserManager>();
				}
				else
				{
					x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use<DefaultUserManager>();
				}
			}
			else
			{
				// Load UserManager type from config
				x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use(LoadUserManagerFromType(userManagerTypeName));
			}
		}

		private UserManager LoadUserManagerFromType(string typeName)
		{
			Type userManagerType = typeof(UserManager);
			Type reflectedType = Type.GetType(typeName);

			if (reflectedType.IsSubclassOf(userManagerType))
			{
				return (UserManager)reflectedType.Assembly.CreateInstance(reflectedType.FullName);
			}
			else
			{
				throw new SecurityException(null, "The type {0} specified in the userManagerType web.config setting is not an instance of a UserManager class", typeName);
			}
		}

		private static IRepository LoadRepositoryFromType(string typeName)
		{
			Type userManagerType = typeof(IRepository);
			Type reflectedType = Type.GetType(typeName);

			if (reflectedType.IsSubclassOf(userManagerType))
			{
				return (IRepository)reflectedType.Assembly.CreateInstance(reflectedType.FullName);
			}
			else
			{
				throw new SecurityException(null, "The type {0} specified in the repositoryType web.config setting is not an instance of a IRepository.", typeName);
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
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use<NHibernateRepository>();
				});
			}

			IRepository repository = ObjectFactory.GetInstance<IRepository>();
			repository.Startup(dataStoreType, connectionString, enableCache);
			return repository;
		}

		public static void DisposeRepository()
		{
			ObjectFactory.GetInstance<IRepository>().Dispose();
		}
	}
}
