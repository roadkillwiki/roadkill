using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.MVC;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using StructureMap;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(LocatorStartup), "StartMVC")]
[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(LocatorStartup), "StartWebApi")]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(LocatorStartup), "End")]

namespace Roadkill.Core.DependencyResolution
{
	// This class does the following:
	// - Is called on app startup
	// - Creates a new Container that uses DefaultRegistry, which does the real work.
	// - Uses a new StructureMapServiceLocator for the service locator
	// - Uses a StructureMapScopeModule HttpModule to create a new container per request

	public static class LocatorStartup
	{
		public static StructureMapServiceLocator Locator { get; set; }

		public static void StartMVC()
		{
			StartMVCInternal(new RoadkillRegistry(new FullTrustConfigReaderWriter("")), true);
		}

		internal static void StartMVCInternal(RoadkillRegistry registry, bool isWeb)
		{
			IContainer container = new Container(c => c.AddRegistry(registry));		
			Locator = new StructureMapServiceLocator(container, isWeb);

			// MVC locator
			DependencyResolver.SetResolver(Locator);
			DynamicModuleUtility.RegisterModule(typeof(StructureMapHttpModule));
		}

		internal static void StartWebApi()
		{
			// WebApi locator
			GlobalConfiguration.Configuration.DependencyResolver = Locator;
		}

		// Must be run **after** the app has started/initialized via WebActivor
		public static void ConfigureAdditionalMVC(IContainer container, ApplicationSettings appSettings)
		{
			// MVC attributes
			var mvcProvider = new MvcAttributeProvider(container);
			FilterProviders.Providers.Add(mvcProvider); // attributes

			// WebAPI attributes
			var webApiProvider = new MvcAttributeProvider(GlobalConfiguration.Configuration.Services.GetFilterProviders(), container);
			GlobalConfiguration.Configuration.Services.Add(typeof(System.Web.Http.Filters.IFilterProvider), webApiProvider);

			// Models with ModelBinding that require DI
			ModelBinders.Binders.Add(typeof(UserViewModel), new UserViewModelModelBinder());
			ModelBinders.Binders.Add(typeof(SettingsViewModel), new SettingsViewModelBinder());

			// Attachments path
			AttachmentRouteHandler.RegisterRoute(appSettings, RouteTable.Routes, container.GetInstance<IFileService>());
		}

		public static void End()
		{
			Locator.Dispose();
		}
	}
}