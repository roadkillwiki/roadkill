using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Roadkill.Core.Attachments;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.DI;
using Roadkill.Core.Logging;
using Roadkill.Core.Managers;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebViewPages;
using Roadkill.Core.Plugins;
using Roadkill.Core.Plugins.BuiltIn;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Query;

namespace Roadkill.Core
{
	public class DependencyManager
	{
		//
		// The dependency chain is:
		//
		// - IRepository relies on ApplicationSettings
		//   - LightSpeedRepository creates its own instances of IUnitOfWork
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
				// * _All_ Roadkill MVC controllers are new'd up by MvcDependencyResolver so dependencies are injected into them
				// * Some view models are new'd up by custom MvcModelBinders so dependencies are injected into them
				// * MVC Attributes are injected using setter injection
				// * All views use RoadkillViewPage which is setter injected.
				// * All layout views use RoadkillLayoutPage which uses bastard injection (as master pages are part of ASP.NET and not MVC) 
				//

				DependencyResolver.SetResolver(new MvcDependencyResolver()); // views and controllers
				FilterProviders.Providers.Add(new MvcAttributeProvider()); // attributes
				ModelBinders.Binders.Add(typeof(UserSummary), new UserSummaryModelBinder()); // models needing DI
				ModelBinders.Binders.Add(typeof(SettingsSummary), new SettingsSummaryModelBinder());

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

			// Temp for custom variable plugins
			x.For<MathJax>().HybridHttpOrThreadLocalScoped();

			// Cache
			x.For<ListCache>().Singleton();
			x.For<PageSummaryCache>().Singleton();
		}

		private void Scan(IAssemblyScanner scanner)
		{
			scanner.TheCallingAssembly();
			scanner.SingleImplementationsOfInterface();
			scanner.WithDefaultConventions();

			// User manager plugins
			string userManagerPluginPath = _applicationSettings.UserManagerPluginsPath;
			if (Directory.Exists(userManagerPluginPath))
				Directory.CreateDirectory(userManagerPluginPath);

			scanner.AssembliesFromPath(userManagerPluginPath);

			// Custom variable plugins
			string customVariablePluginPath = _applicationSettings.CustomVariablePluginsBinPath;
			PluginFactory.CopyCustomVariablePlugins(_applicationSettings);
			if (!Directory.Exists(customVariablePluginPath))
				Directory.CreateDirectory(customVariablePluginPath);

			foreach (string subDirectory in Directory.GetDirectories(customVariablePluginPath))
			{
				scanner.AssembliesFromPath(subDirectory);
			}
			scanner.AddAllTypesOf<CustomVariablePlugin>();

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

			// Emails
			scanner.AddAllTypesOf<SignupEmail>();
			scanner.AddAllTypesOf<ResetPasswordEmail>();

			// Cache
			scanner.AddAllTypesOf<ListCache>();
			scanner.AddAllTypesOf<PageSummaryCache>();
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
				x.For<UserManagerBase>().HybridHttpOrThreadLocalScoped().Use<FormsAuthUserManager>();
			}

			// Setter inject the various MVC objects that can't have constructors
			x.SetAllProperties(y => y.OfType<IControllerAttribute>());
			x.SetAllProperties(y => y.TypeMatches(t => t == typeof(RoadkillViewPage<>)));
			x.SetAllProperties(y => y.TypeMatches(t => t == typeof(RoadkillLayoutPage)));
		}
	}
}