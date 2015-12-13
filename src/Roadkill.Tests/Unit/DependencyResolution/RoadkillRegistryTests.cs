using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Attachments;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Core.Domain.Export;
using Roadkill.Core.Email;
using Roadkill.Core.Import;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Mvc.WebViewPages;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;
using StructureMap;

namespace Roadkill.Tests.Unit.DependencyResolution
{
	[TestFixture]
	[Category("Unit")]
	public class RoadkillRegistryTests
	{
		private IContainer CreateContainer()
		{
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub());
			var container = new Container(registry);
			return container;
		}

		private void AssertDefaultType<TParent, TConcrete>()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			TParent instance = container.GetInstance<TParent>();

			// Assert
			Assert.That(instance, Is.TypeOf<TConcrete>());
		}

		// Appsettings
		[Test]
		public void should_get_application_settings_from_config_reader_instance()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			ConfigReaderWriter configReader = container.GetInstance<ConfigReaderWriter>();
			ApplicationSettings settings = container.GetInstance<ApplicationSettings>();

			// Assert
			Assert.That(settings, Is.Not.Null);
			Assert.That(configReader, Is.TypeOf<ConfigReaderWriterStub>());
		}

		// Plugins
		[Test]
		public void should_register_default_pluginfactory()
		{
			// Arrange + Act + Assert
			AssertDefaultType<IPluginFactory, PluginFactory>();
		}

		// Repositories
		[Test]
		public void should_use_lightspeedrepository_by_default()
		{
			// Arrange + Act + Assert
			AssertDefaultType<IRepository, LightSpeedRepository>();
		}

		[Test]
		public void should_load_repositoryfactory_by_default()
		{
			// Arrange + Act + Assert
			AssertDefaultType<IRepositoryFactory, RepositoryFactory>();
		}

		[Test]
		public void MongoDB_databaseType_should_load_repository()
		{
			// Arrange
			var settings = new ApplicationSettings();
			settings.DatabaseName = "MongoDB";

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act

			// Assert
			Assert.That(container.GetInstance<IRepository>(), Is.TypeOf(typeof(MongoDBRepository)));
		}

		// Context
		[Test]
		public void should_use_usercontext_by_default()
		{
			// Arrange + Act + Assert
			AssertDefaultType<IUserContext, UserContext>();
		}

		// Emails + cache
		[Test]
		public void should_register_email_types()
		{
			// Arrange + Act + Assert
			AssertDefaultType<SignupEmail, SignupEmail>();
			AssertDefaultType<ResetPasswordEmail, ResetPasswordEmail>();
		}

		[Test]
		public void should_register_cache_types()
		{
			// Arrange + Act + Assert
			AssertDefaultType<ListCache, ListCache>();
			AssertDefaultType<PageViewModelCache, PageViewModelCache>();
			AssertDefaultType<ObjectCache, MemoryCache>();
			AssertDefaultType<IPluginCache, SiteCache>();
		}

		[Test]
		public void object_cache_should_be_singleton_roadkill_memorycache()
		{
			// Arrange
			var container = CreateContainer();

			// Act
			var objectCache1 = container.GetInstance<ObjectCache>();
			var objectCache2 = container.GetInstance<ObjectCache>();

			// Assert
			Assert.That(objectCache1, Is.EqualTo(objectCache2));

			MemoryCache memoryCache = objectCache1 as MemoryCache;
			Assert.That(memoryCache, Is.Not.Null);
			Assert.That(memoryCache.Name, Is.EqualTo("Roadkill"));
		}

		[Test]
		public void cache_types_should_be_singletons()
		{
			// Arrange
			var container = CreateContainer();

			// Act
			var listCache1 = container.GetInstance<ListCache>();
			var listCache2 = container.GetInstance<ListCache>();

			var siteCache1 = container.GetInstance<SiteCache>();
			var siteCache2 = container.GetInstance<SiteCache>();

			var pageViewModelCache1 = container.GetInstance<PageViewModelCache>();
			var pageViewModelCache2 = container.GetInstance<PageViewModelCache>();

			// Assert
			Assert.That(listCache1, Is.EqualTo(listCache2));
			Assert.That(siteCache1, Is.EqualTo(siteCache2));
			Assert.That(pageViewModelCache1, Is.EqualTo(pageViewModelCache2));
		}

		// Services, including AbstractClassConvention
		[Test]
		public void should_register_concrete_servicebase_classes()
		{
			// Arrange
			var settings = new ApplicationSettings();
			settings.LdapConnectionString = "LDAP://dc=roadkill.org"; // for ActiveDirectoryUserService
			settings.AdminRoleName = "admins";
			settings.EditorRoleName = "editors";

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act
			IEnumerable<string> serviceTypeNames = container.GetAllInstances<ServiceBase>().ToList().Select(x => x.GetType().Name);

			// Assert
			Assert.That(serviceTypeNames, Contains.Item("SearchService"));
			Assert.That(serviceTypeNames, Contains.Item("PageHistoryService"));
			Assert.That(serviceTypeNames, Contains.Item("PageService"));
			Assert.That(serviceTypeNames, Contains.Item("FormsAuthUserService"));
			Assert.That(serviceTypeNames, Contains.Item("ActiveDirectoryUserService"));
		}

		// Text parsers
		[Test]
		public void should_register_text_and_tokenparsers()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			MarkupConverter markupConverter = container.GetInstance<MarkupConverter>();
			CustomTokenParser tokenParser = container.GetInstance<CustomTokenParser>();

			// Assert
			Assert.That(markupConverter, Is.TypeOf<MarkupConverter>());
			Assert.That(tokenParser, Is.TypeOf<CustomTokenParser>());
		}

		// MVC + controllers
		[Test]
		public void should_register_IRoadkillController_instances()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			IEnumerable<IRoadkillController> controllers = container.GetAllInstances<IRoadkillController>();

			// Assert
			Assert.That(controllers.Count(), Is.EqualTo(15));
		}

		[Test]
		public void should_register_ApiControllerBase_instances()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			IEnumerable<ApiControllerBase> controllers = container.GetAllInstances<ApiControllerBase>();

			// Assert
			Assert.That(controllers.Count(), Is.EqualTo(4));
		}

		[Test]
		public void should_register_ConfigurationTesterController_instance()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			IEnumerable<ConfigurationTesterController> controllers = container.GetAllInstances<ConfigurationTesterController>();

			// Assert
			Assert.That(controllers.Count(), Is.EqualTo(1));
		}

		[Test]
		public void should_fill_iauthorizationattribute_properties()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			IAuthorizationAttribute authorizationAttribute = container.GetInstance<AdminRequiredAttribute>();

			// Assert
			Assert.That(authorizationAttribute.AuthorizationProvider, Is.Not.Null);
		}

		[Test]
		public void should_register_default_MVC_models()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			UserViewModel userModel = container.GetInstance<UserViewModel>();
			SettingsViewModel settingsModel = container.GetInstance<SettingsViewModel>();
			AttachmentRouteHandler routerHandler = container.GetInstance<AttachmentRouteHandler>();

			// Assert
			Assert.That(userModel, Is.TypeOf<UserViewModel>());
			Assert.That(settingsModel, Is.TypeOf<SettingsViewModel>());
			Assert.That(routerHandler, Is.TypeOf<AttachmentRouteHandler>());
		}

		// Emails

		// Cache

		// Import/Export
		[Test]
		public void should_register_default_importers_and_exporter()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			IWikiImporter wikiImporter = container.GetInstance<IWikiImporter>();
			WikiExporter wikiExporter = container.GetInstance<WikiExporter>();

			// Assert
			Assert.That(wikiImporter, Is.TypeOf<ScrewTurnImporter>());
			Assert.That(wikiExporter, Is.TypeOf<WikiExporter>());
		}

		// ISetterInjected
		[Test]
		public void should_fill_isetterinjected_properties_for_attribute()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			ISetterInjected setterInjected = container.GetInstance<AdminRequiredAttribute>();

			// Assert
			Assert.That(setterInjected.ApplicationSettings, Is.Not.Null);
			Assert.That(setterInjected.Context, Is.Not.Null);
			Assert.That(setterInjected.UserService, Is.Not.Null);
			Assert.That(setterInjected.PageService, Is.Not.Null);
			Assert.That(setterInjected.SettingsService, Is.Not.Null);
		}
		
		// Custom file service and azure
		[Test]
		public void should_use_localfileservice_by_default()
		{
			// Arrange + Act + Assert
			AssertDefaultType<IFileService, LocalFileService>();
		}

		[Test]
		public void should_use_azurefileservice_when_setting_has_azure_true()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UseAzureFileStorage = true;

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act

			// Assert
			Assert.That(container.GetInstance<IFileService>(), Is.TypeOf(typeof(AzureFileService)));
		}

		// User services
		[Test]
		public void should_use_formsauthuserservice_by_default()
		{
			// Arrange + Act + Assert
			AssertDefaultType<UserServiceBase, FormsAuthUserService>();
		}

		[Test]
		public void should_load_custom_userservice_using_short_type_format()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UserServiceType = "Roadkill.Plugins.TestUserService, Roadkill.Plugins";
			Console.WriteLine(settings.UserServiceType);
			settings.PluginsBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

			// Act
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act
			UserServiceBase userService = container.GetInstance<UserServiceBase>();
			Assert.That(userService, Is.Not.Null);
			Assert.That(userService.GetType().AssemblyQualifiedName, Is.StringContaining(settings.UserServiceType));
		}

		[Test]
		public void should_load_custom_userservice_using_assemblyqualifiedname()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UserServiceType = typeof(Roadkill.Plugins.TestUserService).AssemblyQualifiedName;
			Console.WriteLine(settings.UserServiceType);
			settings.PluginsBinPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

			// Act
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act
			UserServiceBase userService = container.GetInstance<UserServiceBase>();
			Assert.That(userService, Is.Not.Null);
			Assert.That(userService.GetType().AssemblyQualifiedName, Is.EqualTo(settings.UserServiceType));
		}

#if !MONO
		[Test]
		public void should_load_activedirectory_userservice_when_usewindowsauth_is_true()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UseWindowsAuthentication = true;
			settings.LdapConnectionString = "LDAP://dc=roadkill.org";
			settings.AdminRoleName = "admins";
			settings.EditorRoleName = "editors";

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act

			// Assert
			Assert.That(container.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(ActiveDirectoryUserService)));
		}
#endif

		// Security related
		[Test]
		public void should_register_default_security_providers()
		{
			// Arrange
			IContainer container = CreateContainer();

			// Act
			IAuthorizationProvider authProvider = container.GetInstance<IAuthorizationProvider>();

			// Assert
			Assert.That(authProvider, Is.TypeOf<AuthorizationProvider>());

#if !MONO
			IActiveDirectoryProvider adProvider = container.GetInstance<IActiveDirectoryProvider>();
			Assert.That(adProvider, Is.TypeOf<ActiveDirectoryProvider>());
#endif
		}
	}
}
