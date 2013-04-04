using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Attachments;
using Roadkill.Core.Logging;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebViewPages;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;
using StructureMap;
using StructureMap.Query;

namespace Roadkill.Core
{
	public class DependencyContainer
	{
		//
		// The dependency chain is:
		//
		// - IConfiguration has no dependencies, and (should) lazy load its two properties on demand
		// - IRepository relies on ApplicationSettings
 		// - UserManager relies on IRepository
		// - RoadkillContext relies on UserManager
		// - ActiveDirectoryManager relies on IActiveDirectoryService
		// - The others can rely on everything above.
		//

		private ApplicationSettings _applicationSettings;
		private IRepository _repository;
		private IUserContext _context;
		private bool _useCustomInstances;
		private bool _hasRunInitialization;

		public DependencyContainer(ApplicationSettings applicationSettings)
		{
			if (applicationSettings == null)
				throw new IoCException("The ApplicationSettings parameter is null", null);

			_applicationSettings = applicationSettings;
			_useCustomInstances = false;
		}

		public DependencyContainer(ApplicationSettings applicationSettings, IRepository repository, IUserContext context)
		{
			if (applicationSettings == null)
				throw new IoCException("The ApplicationSettings parameter is null", null);

			if (repository == null)
				throw new IoCException("The IRepository parameter is null", null);

			if (context == null)
				throw new IoCException("The IRoadkillContext parameter is null", null);

			_applicationSettings = applicationSettings;
			_repository = repository;
			_context = context;
			_useCustomInstances = true;
		}

		public void RegisterTypes()
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
					scanner.AddAllTypesOf<ApplicationSettings>();
					scanner.AddAllTypesOf<IRepository>();
					scanner.AddAllTypesOf<IUserContext>();

					// Managers and services
					scanner.AddAllTypesOf<ServiceBase>();
					scanner.AddAllTypesOf<IPageManager>();
					scanner.AddAllTypesOf<IActiveDirectoryService>();
					scanner.AddAllTypesOf<UserManagerBase>();

					// Text parsers
					scanner.AddAllTypesOf<MarkupConverter>();
					scanner.AddAllTypesOf<CustomTokenParser>();

					// MVC Related
					scanner.AddAllTypesOf<Roadkill.Core.Mvc.Controllers.ControllerBase>();
					scanner.AddAllTypesOf<UserSummary>();
					scanner.AddAllTypesOf<SettingsSummary>();
					scanner.AddAllTypesOf<AttachmentRouteHandler>();
					scanner.AddAllTypesOf<IControllerAttribute>();
					scanner.AddAllTypesOf<RoadkillLayoutPage>();
					scanner.AddAllTypesOf(typeof(RoadkillViewPage<>));
					scanner.ConnectImplementationsToTypesClosing(typeof(RoadkillViewPage<>));

					// Cache
					scanner.AddAllTypesOf<ListCache>();
					scanner.AddAllTypesOf<PageSummaryCache>();
				});

				// Web.config/app settings
				x.For<ApplicationSettings>().Singleton().Use(_applicationSettings);

				// Set the 2 core types to use HTTP/Thread-based lifetimes
				x.For<IRepository>().HybridHttpOrThreadLocalScoped();
				x.For<IUserContext>().HybridHttpOrThreadLocalScoped();

				// Cache
				x.For<ListCache>().Singleton();
				x.For<PageSummaryCache>().Singleton();
			});

			ObjectFactory.Configure(x =>
			{
				if (_useCustomInstances)
				{
					RegisterCustomInstances(x);
				}
				else
				{
					//
					// Default repository, or get it from the DataStoreType
					//
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use<LightSpeedRepository>();

					if (_applicationSettings.DataStoreType.RequiresCustomRepository)
					{
						IRepository customRepository = LoadRepositoryFromType(_applicationSettings.DataStoreType.CustomRepositoryType);
						x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(customRepository);
					}
				}

				//
				// UserManager : Windows authentication, custom or the default
				//
				string userManagerTypeName = _applicationSettings.UserManagerType;

				if (_applicationSettings.UseWindowsAuthentication)
				{
					x.For<UserManagerBase>().HybridHttpOrThreadLocalScoped().Use<ActiveDirectoryUserManager>();
				}
				else if (!string.IsNullOrEmpty(userManagerTypeName))
				{
					InstanceRef userManagerRef = ObjectFactory.Model.InstancesOf<UserManagerBase>().FirstOrDefault(t => t.ConcreteType.FullName == userManagerTypeName);
					x.For<UserManagerBase>().HybridHttpOrThreadLocalScoped().TheDefault.Is.OfConcreteType(userManagerRef.ConcreteType);
				}
				else
				{
					x.For<UserManagerBase>().HybridHttpOrThreadLocalScoped().Use<FormsAuthenticationUserManager>();
				}

				x.SetAllProperties(y => y.OfType<IControllerAttribute>());
				x.SetAllProperties(y => y.TypeMatches(t => t == typeof(RoadkillViewPage<>)));
				x.SetAllProperties(y => y.TypeMatches(t => t == typeof(RoadkillLayoutPage)));
			});

			_repository = ObjectFactory.GetInstance<IRepository>();
			_repository.Startup(_applicationSettings.DataStoreType,
								_applicationSettings.ConnectionString,
								_applicationSettings.UseObjectCache);

			Log.ConfigureLogging(_applicationSettings);
			_hasRunInitialization = true;
		}

		private void RegisterCustomInstances(ConfigurationExpression x)
		{
			// Config
			x.For<ApplicationSettings>().Singleton().Use(_applicationSettings);

			// Repository
			x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(_repository);

			// Context
			x.For<IUserContext>().HybridHttpOrThreadLocalScoped().Use(_context);
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
				// *All* Roadkill MVC controllers are new'd up by a controller factory so dependencies are injected into them
				// Some view models are new'd up by a custom object factory so dependencies are injected into them
				// Attributes are injected using setter injection
				// All views use RoadkillViewPage which is setter injected.

				DependencyResolver.SetResolver(new MvcDependencyResolver()); // views and controllers
				FilterProviders.Providers.Add(new MvcAttributeProvider()); // attributes
				ModelBinders.Binders.Add(typeof(UserSummary), new UserSummaryModelBinder()); // models needing DI
				ModelBinders.Binders.Add(typeof(SettingsSummary), new SettingsSummaryModelBinder());

				// Attachments path
				AttachmentRouteHandler.RegisterRoute(_applicationSettings);
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

				IRepository repository = DependencyContainer.ChangeRepository(dataStoreType, connectionString, false);

				System.Configuration.Configuration config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
				RoadkillSection section = config.GetSection("roadkill") as RoadkillSection;

				// Only create the Schema if not already installed otherwise just a straight TestConnection
				bool createSchema = !section.Installed;
				repository.TestConnection(dataStoreType, connectionString);
				return "";
			}
			catch (Exception e)
			{
				return e.ToString();
			}
		}

		/// <summary>
		/// Gets the current instance of T from the IoC, or returns null if doesn't exist.
		/// </summary>
		public static T GetInstance<T>()
		{
			return ObjectFactory.TryGetInstance<T>();
		}

		public static object GetInstance(Type instanceType)
		{
			// See:
			// http://codebetter.com/jeremymiller/2011/01/23/if-you-are-using-structuremap-with-mvc3-please-read-this/
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
	}
}