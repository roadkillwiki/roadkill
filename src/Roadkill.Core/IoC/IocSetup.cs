using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Files;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Pipeline;
using StructureMap.Query;
using ControllerBase = System.Web.Mvc.ControllerBase;

namespace Roadkill.Core
{
	public class IoCSetup
	{
		//
		// The dependency chain is:
		//
		// - IConfiguration has no dependencies, and (should) lazy load its two properties on demand
		// - IRepository relies on IConfigurationContainer 
		// - RoadkillContext relies on UserManager, which relies on IRepository
		// - ActiveDirectoryManager relies on IActiveDirectoryService
		// - The others can rely on everything above.
		//

		private IConfigurationContainer _config;
		private IRepository _repository;
		private IRoadkillContext _context;
		private bool _useCustomInstances;
		private bool _hasRunInitialization;

		public IoCSetup()
		{
			_useCustomInstances = false;
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
			_useCustomInstances = true;
		}

		public void Run()
		{
			string pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Plugins");

			ObjectFactory.Initialize(x =>
			{
				x.Scan(scanner =>
				{
					scanner.TheCallingAssembly();
					scanner.SingleImplementationsOfInterface();
					scanner.WithDefaultConventions();

					// Plugin UserManagers
					if (Directory.Exists(pluginPath))
						scanner.AssembliesFromPath(pluginPath);

					// Config, repository, context
					scanner.AddAllTypesOf<IConfigurationContainer>();
					scanner.AddAllTypesOf<IRepository>();
					scanner.AddAllTypesOf<IRoadkillContext>();

					// Managers and services
					scanner.AddAllTypesOf<ServiceBase>();
					scanner.AddAllTypesOf<IPageManager>();
					scanner.AddAllTypesOf<IActiveDirectoryService>();
					scanner.AddAllTypesOf<UserManager>();

					// Text parsers
					scanner.AddAllTypesOf<MarkupConverter>();
					scanner.AddAllTypesOf<CustomTokenParser>();

					// MVC Related
					scanner.AddAllTypesOf<Roadkill.Core.Controllers.ControllerBase>();
					scanner.AddAllTypesOf<UserSummary>();
					scanner.AddAllTypesOf<SettingsSummary>();
					scanner.AddAllTypesOf<AttachmentRouteHandler>();
				});

				// Set the 3 core types to use HTTP/Thread-based lifetimes
				x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped();
				x.For<IRepository>().HybridHttpOrThreadLocalScoped();
				x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped();
			});

			ObjectFactory.Configure(x =>
			{
				if (_useCustomInstances)
				{
					RegisterCustomInstances(x);
				}
				else
				{
					_config = ObjectFactory.GetInstance<IConfigurationContainer>();

					//
					// Default repository, or get it from the DataStoreType
					//
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use<LightSpeedRepository>();

					_config.ApplicationSettings.Load();
					if (_config.ApplicationSettings.DataStoreType.RequiresCustomRepository)
					{
						IRepository customRepository = LoadRepositoryFromType(_config.ApplicationSettings.DataStoreType.CustomRepositoryType);
						x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(customRepository);
					}
				}

				//
				// UserManager : Windows authentication, custom or the default
				//
				string userManagerTypeName = _config.ApplicationSettings.UserManagerType;

				if (_config.ApplicationSettings.UseWindowsAuthentication)
				{
					x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use<ActiveDirectoryUserManager>();
				}
				else if (!string.IsNullOrEmpty(userManagerTypeName))
				{
					InstanceRef userManagerRef = ObjectFactory.Model.InstancesOf<UserManager>().FirstOrDefault(t => t.ConcreteType.FullName == userManagerTypeName);
					x.For<UserManager>().HybridHttpOrThreadLocalScoped().TheDefault.Is.OfConcreteType(userManagerRef.ConcreteType);
				}
				else
				{
					x.For<UserManager>().HybridHttpOrThreadLocalScoped().Use<DefaultUserManager>();
				}

			});

			// Let the repositories perform any startup tasks (I'm looking at you, NHibernate)
			_repository = ObjectFactory.GetInstance<IRepository>();
			_repository.Startup(_config.ApplicationSettings.DataStoreType,
								_config.ApplicationSettings.ConnectionString,
								_config.ApplicationSettings.CacheEnabled);

			_hasRunInitialization = true;
		}

		private void RegisterCustomInstances(ConfigurationExpression x)
		{
			// Config
			x.For<IConfigurationContainer>().HybridHttpOrThreadLocalScoped().Use(_config);

			// Repository
			x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(_repository);

			// Context
			x.For<IRoadkillContext>().HybridHttpOrThreadLocalScoped().Use(_context);
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

		private static IRepository LoadRepositoryFromType(string typeName)
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

		public void RegisterMvcFactoriesAndRouteHandlers()
		{
			if (_hasRunInitialization)
			{
				// Some view models are new'd up by a custom object factory so dependencies are injected into them
				if (!ModelBinders.Binders.ContainsKey(typeof(UserSummary)))
					ModelBinders.Binders.Add(typeof(UserSummary), new UserSummaryModelBinder());

				if (!ModelBinders.Binders.ContainsKey(typeof(SettingsSummary)))
					ModelBinders.Binders.Add(typeof(SettingsSummary), new SettingsSummaryModelBinder());

				// *All* Roadkill MVC controllers are new'd up by a controller factory so dependencies are injected into them
				ControllerBuilder.Current.SetControllerFactory(new ControllerFactory());

				// Attachments path
				AttachmentRouteHandler.Register(_config);
			}
			else
			{
				throw new IoCException("Please call Run() to perform IoC initialization before performing MVC setup.", null);
			}
		}

		/// <summary>
		/// This method could be removed and then a refactor into an IUnitOfWork with StructureMap
		/// </summary>
		public static void DisposeRepository()
		{	
			IConfigurationContainer config = ObjectFactory.GetInstance<IConfigurationContainer>();

			// Don't try to dispose a repository if the app isn't installed, as it the repository won't be correctly configured.
			// (as no connection string is set, the Startup doesn't complete and the IUnitOfWork isn't registered with StructureMap)
			if (config.ApplicationSettings.Installed)
			{
				IRepository repository = ObjectFactory.GetInstance<IRepository>();

				if (config.ApplicationSettings.Installed)
				{
					ObjectFactory.GetInstance<IRepository>().Dispose();
				}
			}
		}
	}
}