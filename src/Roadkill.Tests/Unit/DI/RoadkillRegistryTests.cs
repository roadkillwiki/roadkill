using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Core.Import;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;
using StructureMap;
using StructureMap.Query;

namespace Roadkill.Tests.Unit.DI
{
	[TestFixture]
	[Category("Unit")]
	public class RoadkillRegistryTests
	{
		[Test]
		public void Should_Register_Default_Instances()
		{
			// Arrange
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub());
			var container = new Container(registry);

			// Act
			ConfigReaderWriter configReader = container.GetInstance<ConfigReaderWriter>();
			ApplicationSettings settings = container.GetInstance<ApplicationSettings>();
			IRepository repository = container.GetInstance<IRepository>();
			IUserContext context = container.GetInstance<IUserContext>();
			IPageService pageService = container.GetInstance<IPageService>();
			MarkupConverter markupConverter = container.GetInstance<MarkupConverter>();
			CustomTokenParser tokenParser = container.GetInstance<CustomTokenParser>();
			UserViewModel userModel = container.GetInstance<UserViewModel>();
			SettingsViewModel settingsModel = container.GetInstance<SettingsViewModel>();
			AttachmentRouteHandler routerHandler = container.GetInstance<AttachmentRouteHandler>();
			UserServiceBase userManager = container.GetInstance<UserServiceBase>();
			IPluginFactory pluginFactory = container.GetInstance<IPluginFactory>();
			IWikiImporter wikiImporter = container.GetInstance<IWikiImporter>();
			IAuthorizationProvider authProvider = container.GetInstance<IAuthorizationProvider>();
			IActiveDirectoryProvider adProvider = container.GetInstance<IActiveDirectoryProvider>();


			// Assert
			Assert.That(settings, Is.Not.Null);
			Assert.That(configReader, Is.TypeOf<ConfigReaderWriterStub>());
			Assert.That(repository, Is.TypeOf<LightSpeedRepository>());
			Assert.That(context, Is.TypeOf<UserContext>());
			Assert.That(pageService, Is.TypeOf<PageService>());			
			Assert.That(markupConverter, Is.TypeOf<MarkupConverter>());
			Assert.That(tokenParser, Is.TypeOf<CustomTokenParser>());
			Assert.That(userModel, Is.TypeOf<UserViewModel>());
			Assert.That(settingsModel, Is.TypeOf<SettingsViewModel>());
			Assert.That(userManager, Is.TypeOf<FormsAuthUserService>());
			Assert.That(pluginFactory, Is.TypeOf<PluginFactory>());
			Assert.That(wikiImporter, Is.TypeOf<ScrewTurnImporter>());
			Assert.That(authProvider, Is.TypeOf<AuthorizationProvider>());

#if !MONO
			Assert.That(adProvider, Is.TypeOf<ActiveDirectoryProvider>());
#endif
		}

		[Test]
		public void Should_Register_Controller_Instances()
		{
			// Arrange
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub());
			var container = new Container(registry);

			// Act
			IEnumerable<Roadkill.Core.Mvc.Controllers.ControllerBase> controllers = container.GetAllInstances<Roadkill.Core.Mvc.Controllers.ControllerBase>();

			// Assert
			//Assert.That(controllers.Count(), Is.GreaterThanOrEqualTo(9));
		}

		[Test]
		public void Should_Register_Service_Instances_When_Windows_Auth_Enabled()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UseWindowsAuthentication = true;
			settings.LdapConnectionString = "LDAP://123.com";
			settings.EditorRoleName = "editor;";
			settings.AdminRoleName = "admins";

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act
			IEnumerable<ServiceBase> services = container.GetAllInstances<ServiceBase>();

			// Assert
			Assert.That(services.Count(), Is.GreaterThanOrEqualTo(7));
		}

		[Test]
		public void Custom_Configuration_Repository_And_Context_Types_Should_Be_Registered()
		{
			// Arrange
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub());
			var container = new Container(registry);

			// Act
			IRepository repository = container.GetInstance<IRepository>();
			IUserContext context = container.GetInstance<IUserContext>();

			// Assert
			Assert.That(repository, Is.TypeOf<RepositoryMock>());
			Assert.That(context, Is.TypeOf<UserContextStub>());
		}

#if !MONO
		[Test]
		public void WindowsAuth_Should_Register_ActiveDirectoryUserManager()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UseWindowsAuthentication = true;
			settings.LdapConnectionString = "LDAP://123.com";
			settings.EditorRoleName = "editor;";
			settings.AdminRoleName = "admins";

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act
			UserServiceBase usermanager = container.GetInstance<UserServiceBase>();

			// Assert
			Assert.That(usermanager, Is.TypeOf<ActiveDirectoryUserService>());
		}
#endif

		[Test]
		[Ignore("Create LocatorStartupTests for this")]
		public void RegisterMvcFactoriesAndRouteHandlers_Should_Register_Instances()
		{
			// Arrange
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub());
			var container = new Container(registry);

			// Act + Assert
			Assert.That(RouteTable.Routes.Count, Is.EqualTo(1));
			Assert.That(((Route)RouteTable.Routes[0]).RouteHandler, Is.TypeOf<AttachmentRouteHandler>());
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(SettingsViewModel)));
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(UserViewModel)));
		}

		[Test]
		public void Should_Load_Custom_Repository_From_DatabaseName()
		{
			// Arrange
			var settings = new ApplicationSettings();
			settings.DatabaseName = "MongoDB";

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act

			// Assert
			IRepository respository = container.GetInstance<IRepository>();
			Assert.That(respository, Is.TypeOf<MongoDBRepository>());
		}

		[Test]
		public void Should_Fill_ISetterInjected_Properties()
		{
			// Arrange
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub());
			var container = new Container(registry);

			// Act
			ISetterInjected setterInjected = container.GetInstance<AdminRequiredAttribute>();

			// Assert
			Assert.That(setterInjected.ApplicationSettings, Is.Not.Null);
			Assert.That(setterInjected.Context, Is.Not.Null);
			Assert.That(setterInjected.UserService, Is.Not.Null);
			Assert.That(setterInjected.PageService, Is.Not.Null);
			Assert.That(setterInjected.SettingsService, Is.Not.Null);
		}

		[Test]
		public void Should_Fill_IAuthorizationAttribute_Properties()
		{
			// Arrange
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub());
			var container = new Container(registry);

			// Act
			IAuthorizationAttribute authorizationAttribute = container.GetInstance<AdminRequiredAttribute>();

			// Assert
			Assert.That(authorizationAttribute.AuthorizationProvider, Is.Not.Null);
		}

		[Test]
		public void Should_Copy_Plugins()
		{
			// TODO
		}

		[Test]
		public void Should_Load_Custom_UserService_Using_Short_Type_Format()
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
			Assert.That(userService.GetType().AssemblyQualifiedName, Is.EqualTo(settings.UserServiceType));
		}

		[Test]
		public void Should_Load_Custom_UserService_Using_AssemblyQualifiedName()
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

		[Test]
		[Explicit]
		public void MongoDB_databaseType_Should_Load_Repository()
		{
			// Arrange
			var settings = new ApplicationSettings();
			settings.DatabaseName = "MongoDB";

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act

			// Assert
			Assert.That(container.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(FormsAuthUserService)));
		}

		[Test]
		public void Should_Use_FormsAuthUserService_By_Default()
		{
			// Arrange
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub());
			var container = new Container(registry);

			// Act

			// Assert
			Assert.That(container.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(FormsAuthUserService)));
		}

 #if !MONO
		[Test]
		public void Should_Load_ActiveDirectory_UserService_When_UseWindowsAuth_Is_True()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.UseWindowsAuthentication = true;
			settings.LdapConnectionString = "LDAP://dc=roadkill.org";
			settings.AdminRoleName = "editors";
			settings.EditorRoleName = "editors";

			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			// Act

			// Assert
			Assert.That(container.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(ActiveDirectoryUserService)));
		}
#endif
	}
}
