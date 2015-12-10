using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using Roadkill.Core.Attachments;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Domain.Export;
using Roadkill.Core.Email;
using Roadkill.Core.Import;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebViewPages;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;
using Roadkill.Core.Services;
using StructureMap;
using StructureMap.Building;
using StructureMap.Diagnostics;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using StructureMap.Pipeline;
using StructureMap.Query;
using StructureMap.TypeRules;
using StructureMap.Web;
using WebGrease.Css.Extensions;

namespace Roadkill.Core.DependencyResolution.StructureMap
{
	public class RoadkillRegistry : Registry
	{
		private class AbstractClassConvention<T> : IRegistrationConvention
		{
			public void ScanTypes(TypeSet types, Registry registry)
			{
				types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).ForEach(type =>
				{
					if (type.CanBeCastTo<T>())
					{
						registry.For(typeof(T)).LifecycleIs(new UniquePerRequestLifecycle()).Add(type);
					}
				});
			}
		}

		private ApplicationSettings _applicationSettings { get; set; }

		public RoadkillRegistry(ConfigReaderWriter configReader)
		{
			_applicationSettings = configReader.GetApplicationSettings();

			Scan(ScanTypes);
			ConfigureInstances(configReader);
			ConfigureUserService();
			ConfigureFileService();
			ConfigureSetterInjection();
		}

		private void ScanTypes(IAssemblyScanner scanner)
		{
			scanner.TheCallingAssembly();
			scanner.AssembliesFromApplicationBaseDirectory(assembly => assembly.FullName.Contains("Roadkill"));
			scanner.SingleImplementationsOfInterface();
			scanner.WithDefaultConventions();

			// Copy all plugins to the /bin/Plugins folder
			CopyPlugins();

			// Scan plugins: this includes everything e.g repositories, UserService, FileService TextPlugins
			foreach (string subDirectory in Directory.GetDirectories(_applicationSettings.PluginsBinPath))
			{
				scanner.AssembliesFromPath(subDirectory);
			}

			// Plugins
            scanner.With(new AbstractClassConvention<TextPlugin>());
			scanner.With(new AbstractClassConvention<SpecialPagePlugin>());
			scanner.AddAllTypesOf<IPluginFactory>();

			// Config, repository, context
			scanner.AddAllTypesOf<ApplicationSettings>();
			scanner.AddAllTypesOf<IRepository>();
			scanner.AddAllTypesOf<IUserContext>();

			// Services and services
			scanner.With(new AbstractClassConvention<ServiceBase>());
			scanner.With(new AbstractClassConvention<UserServiceBase>());
			scanner.AddAllTypesOf<IPageService>();
			scanner.AddAllTypesOf<IActiveDirectoryProvider>();
			scanner.AddAllTypesOf<IFileService>();
			scanner.AddAllTypesOf<IInstallationService>();

			// Text parsers
			scanner.AddAllTypesOf<MarkupConverter>();
			scanner.AddAllTypesOf<CustomTokenParser>();

			// MVC Related
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

			// Controllers
			scanner.AddAllTypesOf<IRoadkillController>();
			scanner.AddAllTypesOf<ControllerBase>();
			scanner.AddAllTypesOf<ApiControllerBase>();
			scanner.AddAllTypesOf<ConfigurationTesterController>();
		}

		private void CopyPlugins()
		{
			// Copy SpecialPages plugins to the /bin folder
			string pluginsDestPath = _applicationSettings.PluginsBinPath;
			if (!Directory.Exists(pluginsDestPath))
				Directory.CreateDirectory(pluginsDestPath);

			PluginFactory.CopyPlugins(_applicationSettings);
		}

		private void ConfigureInstances(ConfigReaderWriter configReader)
		{
			// Appsettings and reader - these need to go first
			For<ConfigReaderWriter>().Singleton().Use(configReader);
			For<ApplicationSettings>()
				.HybridHttpOrThreadLocalScoped()
				.Use(x => x.TryGetInstance<ConfigReaderWriter>().GetApplicationSettings());

			// Repository
			For<IRepositoryFactory>().HybridHttpOrThreadLocalScoped().Use<RepositoryFactory>();
			For<IRepository>()
				.HybridHttpOrThreadLocalScoped()
				.Use(x => x.TryGetInstance<IRepositoryFactory>().GetRepository(_applicationSettings.DatabaseName, _applicationSettings.ConnectionString));

			// Plugins
			For<IPluginFactory>().Singleton().Use<PluginFactory>();

			// Screwturn importer
			For<IWikiImporter>().Use<ScrewTurnImporter>();

			// Emails
			For<SignupEmail>().Use<SignupEmail>();
			For<ResetPasswordEmail>().Use<ResetPasswordEmail>();

			// Cache
			For<ObjectCache>().Use(new MemoryCache("Roadkill"));
			For<ListCache>().Singleton();
			For<SiteCache>().Singleton();
			For<PageViewModelCache>().Singleton();
			For<IPluginCache>().Use<SiteCache>();

			// Services
			For<IPageService>().HybridHttpOrThreadLocalScoped().Use<PageService>();
			For<IInstallationService>().HybridHttpOrThreadLocalScoped().Use<InstallationService>();

			// Security
			For<IAuthorizationProvider>().Use<AuthorizationProvider>();
			For<IUserContext>().HybridHttpOrThreadLocalScoped();

#if !MONO
			For<IActiveDirectoryProvider>().Use<ActiveDirectoryProvider>();
#endif
		}

		private void ConfigureSetterInjection()
		{
			Policies.SetAllProperties(x => x.OfType<ISetterInjected>());
			Policies.SetAllProperties(x => x.OfType<IAuthorizationAttribute>());
			Policies.SetAllProperties(x => x.TypeMatches(t => t == typeof (RoadkillViewPage<>)));
			Policies.SetAllProperties(x => x.TypeMatches(t => t == typeof (RoadkillLayoutPage)));

			// Setter inject the *internal* properties for the text plugins
			For<TextPlugin>().OnCreationForAll("set plugin cache", (ctx, plugin) => plugin.PluginCache = ctx.GetInstance<IPluginCache>());
			For<TextPlugin>().OnCreationForAll("set plugin repository", (ctx, plugin) => plugin.Repository = ctx.GetInstance<IRepository>());
		}

		private void ConfigureFileService()
		{
			if (_applicationSettings.UseAzureFileStorage)
			{
				For<IFileService>()
					.HybridHttpOrThreadLocalScoped()
					.Use<AzureFileService>();
			}
			else
			{
				For<IFileService>()
					.HybridHttpOrThreadLocalScoped()
					.Use<LocalFileService>();
			}
		}

		private void ConfigureUserService()
		{
			// Windows authentication, custom or the default FormsAuth
			string userServiceTypeName = _applicationSettings.UserServiceType;

			if (_applicationSettings.UseWindowsAuthentication)
			{
#if !MONO
				For<UserServiceBase>()
					.HybridHttpOrThreadLocalScoped()
					.Use<ActiveDirectoryUserService>();
#endif
			}
			else if (!string.IsNullOrEmpty(userServiceTypeName))
			{
				try
				{
					Type userServiceType = Type.GetType(userServiceTypeName, false, false);
					if (userServiceType == null)
						throw new IoCException(null, "Unable to find UserService type {0}. Make sure you use the AssemblyQualifiedName.", userServiceTypeName);

					For<UserServiceBase>()
						.Use("Inject custom UserService", context =>
						{
							var obj = context.GetInstance(userServiceType);
							
							return (UserServiceBase) context.GetInstance(userServiceType);
						});
				}
				catch (StructureMapBuildException)
				{
					throw new IoCException(null, "Unable to find UserService type {0}", userServiceTypeName);
				}
			}
			else
			{
				For<UserServiceBase>()
					.HybridHttpOrThreadLocalScoped()
					.Use<FormsAuthUserService>();
			}
		}
	}
}