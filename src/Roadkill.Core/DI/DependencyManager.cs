using Roadkill.Core.Attachments;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.DI.Mvc;
using Roadkill.Core.Domain.Export;
using Roadkill.Core.Email;
using Roadkill.Core.Import;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebViewPages;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;
using Roadkill.Core.Services;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Query;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace Roadkill.Core.DI
{
	public class DependencyManager
	{
		//
		// The dependency chain is:
		//
		// - IRepository relies on ApplicationSettings
		//   - LightSpeedRepository creates its own instances of IUnitOfWork
 		// - UserService relies on IRepository
		// - RoadkillContext relies on UserService
		// - ActiveDirectoryService relies on IActiveDirectoryProvider
		// - The others can rely on everything above.
		//

		private ApplicationSettings _applicationSettings;
		private IRepository _repository;
		private IUserContext _context;
		private bool _useCustomInstances;
		private bool _hasRunInitialization;

		public DependencyManager(ApplicationSettings applicationSettings)
		{
			if (applicationSettings == null)
				throw new IoCException("The ApplicationSettings parameter is null", null);

			_applicationSettings = applicationSettings;
			_useCustomInstances = false;
		}

		public DependencyManager(ApplicationSettings applicationSettings, IRepository repository, IUserContext context)
		{
			if (applicationSettings == null)
				throw new IoCException("The ApplicationSettings parameter is null", null);

			if (repository == null)
				throw new IoCException("The IRepository parameter is null", null);

			if (context == null)
				throw new IoCException("The IRoadkillContext parameter is null", null);

			if (applicationSettings.Installed && string.IsNullOrEmpty(applicationSettings.ConnectionString))
				throw new DatabaseException("The configuration file's connection string is empty (and installed=true).", null);

			_applicationSettings = applicationSettings;
			_repository = repository;
			_context = context;
			_useCustomInstances = true;
		}

		public void Configure()
		{
			ObjectFactory.Initialize(Initialize);
			ObjectFactory.Configure(Configure);

			// Get the latest instance of the repository, it may have changed after configuration
			_repository = ObjectFactory.GetInstance<IRepository>();

			// Tell the current repository to do its thing
			_repository.Startup(_applicationSettings.DataStoreType,
								_applicationSettings.ConnectionString,
								_applicationSettings.UseObjectCache);

			_hasRunInitialization = true;
		}

		public void ConfigureMvc()
		{
			if (_hasRunInitialization)
			{
				//
				// * **All** Roadkill MVC controllers are new'd up by MvcDependencyResolver so dependencies are injected into them
				// * Some view models are new'd up by custom MvcModelBinders so dependencies are injected into them
				// * MVC Attributes are injected using setter injection
				// * All views use RoadkillViewPage which is setter injected.
				// * All layout views use RoadkillLayoutPage which uses bastard injection (as master pages are part of ASP.NET and not MVC) 
				//

				MvcDependencyResolver mvcResolver = new MvcDependencyResolver();

				GlobalConfiguration.Configuration.DependencyResolver = mvcResolver; // web api
				GlobalConfiguration.Configuration.Services.Add(typeof(System.Web.Http.Filters.IFilterProvider), new MvcAttributeProvider(GlobalConfiguration.Configuration.Services.GetFilterProviders()));// web api

				DependencyResolver.SetResolver(mvcResolver); // views and controllers
				FilterProviders.Providers.Add(new MvcAttributeProvider()); // attributes
				ModelBinders.Binders.Add(typeof(UserViewModel), new UserViewModelModelBinder()); // models needing DI
				ModelBinders.Binders.Add(typeof(SettingsViewModel), new SettingsViewModelBinder());

				// Attachments path
				AttachmentRouteHandler.RegisterRoute(_applicationSettings, RouteTable.Routes);
			}
			else
			{
				throw new IoCException("Please call Run() to perform IoC initialization before performing MVC setup.", null);
			}
		}

		private void Initialize(IInitializationExpression x)
		{
			x.Scan(Scan);

			// Web.config/app settings
			x.For<ApplicationSettings>().Singleton().Use(_applicationSettings);

			// Set the 2 core types to use HTTP/Thread-based lifetimes
			x.For<IRepository>().HybridHttpOrThreadLocalScoped();
			x.For<IUserContext>().HybridHttpOrThreadLocalScoped();

			// Plugins
			x.For<IPluginFactory>().Singleton().Use<PluginFactory>();

			// Screwturn importer
			x.For<IWikiImporter>().Use<ScrewTurnImporter>();

			// Cache
			x.For<ObjectCache>().Use(new MemoryCache("Roadkill"));
			x.For<ListCache>().Singleton();
			x.For<SiteCache>().Singleton();
			x.For<PageViewModelCache>().Singleton();
			x.For<IPluginCache>().Use<SiteCache>();

			// Site sttings
			x.For<ConfigReaderWriter>().Use<FullTrustConfigReaderWriter>();

			// Security
			x.For<IAuthorizationProvider>().Use<AuthorizationProvider>();
			x.For<IActiveDirectoryProvider>().Use<ActiveDirectoryProvider>();
		}

		private void Scan(IAssemblyScanner scanner)
		{
			scanner.TheCallingAssembly();
			scanner.SingleImplementationsOfInterface();
			scanner.WithDefaultConventions();

			// Copy all plugins to the /bin/Plugins folder
			CopyPlugins();

			// Scan plugins
			foreach (string subDirectory in Directory.GetDirectories(_applicationSettings.PluginsBinPath))
			{
				scanner.AssembliesFromPath(subDirectory);
			}

			// UserServiceBase is scanned below
			// Scan for TextPlugins
			scanner.AddAllTypesOf<TextPlugin>();
			
			// Scan for SpecialPages
			scanner.AddAllTypesOf<SpecialPagePlugin>();

			// The pluginfactory
			scanner.AddAllTypesOf<IPluginFactory>();

			// Config, repository, context
			scanner.AddAllTypesOf<ApplicationSettings>();
			scanner.AddAllTypesOf<IRepository>();
			scanner.AddAllTypesOf<IUserContext>();

			// Services and services
			scanner.AddAllTypesOf<ServiceBase>();
			scanner.AddAllTypesOf<IPageService>();
			scanner.AddAllTypesOf<IActiveDirectoryProvider>();
			scanner.AddAllTypesOf<UserServiceBase>();
			scanner.AddAllTypesOf<IFileService>();

			// Text parsers
			scanner.AddAllTypesOf<MarkupConverter>();
			scanner.AddAllTypesOf<CustomTokenParser>();

			// MVC Related
			scanner.AddAllTypesOf<Roadkill.Core.Mvc.Controllers.Api.ApiControllerBase>();
			scanner.AddAllTypesOf<Roadkill.Core.Mvc.Controllers.ControllerBase>();
			scanner.AddAllTypesOf<UserViewModel>();
			scanner.AddAllTypesOf<SettingsViewModel>();
			scanner.AddAllTypesOf<AttachmentRouteHandler>();
			scanner.AddAllTypesOf<ISetterInjected>();
			scanner.AddAllTypesOf<IAuthorizationAttribute>();
			scanner.AddAllTypesOf<RoadkillLayoutPage>();
			scanner.AddAllTypesOf(typeof(RoadkillViewPage<>));
			scanner.ConnectImplementationsToTypesClosing(typeof(RoadkillViewPage<>));

			// Emails
			scanner.AddAllTypesOf<SignupEmail>();
			scanner.AddAllTypesOf<ResetPasswordEmail>();

			// Cache
			scanner.AddAllTypesOf<ListCache>();
			scanner.AddAllTypesOf<PageViewModelCache>();

			// Export
			scanner.AddAllTypesOf<WikiExporter>();
		}

		private void Configure(ConfigurationExpression x)
		{
			if (_useCustomInstances)
			{
				// Config
				x.For<ApplicationSettings>().Singleton().Use(_applicationSettings);

				// Repository
				x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(_repository);

				// Context
				x.For<IUserContext>().HybridHttpOrThreadLocalScoped().Use(_context);
			}
			else
			{
				//
				// Default repository, or get it from the DataStoreType
				//
				x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use<LightSpeedRepository>();

				if (_applicationSettings.DataStoreType.RequiresCustomRepository)
				{
					IRepository customRepository = RepositoryManager.LoadRepositoryFromType(_applicationSettings.DataStoreType.CustomRepositoryType);
					x.For<IRepository>().HybridHttpOrThreadLocalScoped().Use(customRepository);
				}
			}

			//
			// UserService : Windows authentication, custom or the default
			//
			string userServiceTypeName = _applicationSettings.UserServiceType;

			if (_applicationSettings.UseWindowsAuthentication)
			{
				x.For<UserServiceBase>().HybridHttpOrThreadLocalScoped().Use<ActiveDirectoryUserService>();
			}
			else if (!string.IsNullOrEmpty(userServiceTypeName))
			{
				InstanceRef userServiceRef = ObjectFactory.Model.InstancesOf<UserServiceBase>().FirstOrDefault(t => t.ConcreteType.FullName == userServiceTypeName);
				if (userServiceRef == null)
				{
					var instances = ObjectFactory.Model.InstancesOf<UserServiceBase>();
					string debugMessage = string.Join(Environment.NewLine, instances.Select(t => t.ConcreteType.FullName).ToArray());
					throw new ConfigurationException(null, "Unable to find custom user service type '{0}' - I only have these types: \n\n{1}", userServiceTypeName, debugMessage);
				}

				x.For<UserServiceBase>().HybridHttpOrThreadLocalScoped().TheDefault.Is.OfConcreteType(userServiceRef.ConcreteType);
			}
			else
			{
				x.For<UserServiceBase>().HybridHttpOrThreadLocalScoped().Use<FormsAuthUserService>();
			}

			// IFileService : Local or Azure or Custom
			if (_applicationSettings.UseAzureFileStorage)
			{
				x.For<IFileService>().HybridHttpOrThreadLocalScoped().Use<AzureFileService>();
			}
			else
			{
				x.For<IFileService>().HybridHttpOrThreadLocalScoped().Use<LocalFileService>();
			}
			

			// Setter inject the various MVC objects that can't have constructors
			x.SetAllProperties(y => y.OfType<ISetterInjected>());
			x.SetAllProperties(y => y.OfType<IAuthorizationAttribute>());
			x.SetAllProperties(y => y.TypeMatches(t => t == typeof(RoadkillViewPage<>)));
			x.SetAllProperties(y => y.TypeMatches(t => t == typeof(RoadkillLayoutPage)));

			// Setter inject the *internal* properties for the text plugins
			x.For<TextPlugin>().OnCreationForAll((ctx, plugin) => plugin.PluginCache = ctx.GetInstance<IPluginCache>());
			x.For<TextPlugin>().OnCreationForAll((ctx, plugin) => plugin.Repository = ctx.GetInstance<IRepository>());
		}

		private void CopyPlugins()
		{
			PluginFactory pluginFactory = new PluginFactory(); // registered as a singleton later

			// Copy SpecialPages plugins to the /bin folder
			string pluginsDestPath = _applicationSettings.PluginsBinPath;
			if (!Directory.Exists(pluginsDestPath))
				Directory.CreateDirectory(pluginsDestPath);

			pluginFactory.CopyPlugins(_applicationSettings);
		}
	}
}