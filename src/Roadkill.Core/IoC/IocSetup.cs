using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Files;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Pipeline;
using ControllerBase = System.Web.Mvc.ControllerBase;

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
			// The dependency chain is:
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
				RegisterRouterHandler(x);
				RegisterUserManager(x);
				x.For<IPageManager>().HybridHttpOrThreadLocalScoped().Use<PageManager>();
			});

			// GetInstance only works once the Configure() method completes
			_repository = ObjectFactory.GetInstance<IRepository>();
			_context = ObjectFactory.GetInstance<IRoadkillContext>();

			// Let the repositories perform any startup tasks (I'm looking at you, NHibernate)
			_repository.Startup(_config.ApplicationSettings.DataStoreType,
								_config.ApplicationSettings.ConnectionString,
								_config.ApplicationSettings.CacheEnabled);

			// *All* Roadkill MVC controllers are new'd up by a controller factory so dependencies are injected into them
			ControllerBuilder.Current.SetControllerFactory(new ControllerFactory());
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

			// Some view models are new'd up by a custom object factory so dependencies are injected into them
			ModelBinders.Binders.Add(typeof(UserSummary), new UserSummaryModelBinder());
			ModelBinders.Binders.Add(typeof(SettingsSummary), new SettingsSummaryModelBinder());
		}

		private void RegisterRouterHandler(IInitializationExpression x)
		{
			// Register the ~/attachments/ route
			// AttachmentRouteHandler requires IConfigurationContainer in its constructors
			x.For<AttachmentRouteHandler>().Use<AttachmentRouteHandler>();
		}

		private void RegisterUserManager(IInitializationExpression x)
		{
			string userManagerTypeName = _config.ApplicationSettings.UserManagerType;
			if (string.IsNullOrEmpty(userManagerTypeName))
			{
				// Load SQL usermanager or windows auth one by default
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

		private static IRepository LoadRepositoryFromType(string typeName)
		{
			Type repositoryType = typeof(IRepository);
			Type reflectedType = Type.GetType(typeName);

			if (repositoryType.IsAssignableFrom(reflectedType))
			{
				return (IRepository) ObjectFactory.GetInstance(reflectedType);
			}
			else
			{
				throw new IoCException(null, "The type {0} specified in the repositoryType web.config setting is not an instance of a IRepository.", typeName);
			}
		}

		/// <summary>
		/// This method could be removed and then a refactor into an IUnitOfWork with StructureMap
		/// </summary>
		public static void DisposeRepository()
		{
			// Don't try to dispose a repository if the app isn't installed, as it the repository won't be correctly configured.
			IConfigurationContainer config = ObjectFactory.GetInstance<IConfigurationContainer>();
			if (config.ApplicationSettings.Installed)
			{
				ObjectFactory.GetInstance<IRepository>().Dispose();
			}
		}
	}
}
