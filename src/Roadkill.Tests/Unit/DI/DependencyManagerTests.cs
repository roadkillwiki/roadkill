using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
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
using Roadkill.Tests.Setup;
using Roadkill.Tests.Unit.StubsAndMocks;
using StructureMap;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class RoadkillRegistryTests
	{
		[SetUp]
		public void Setup()
		{
			RoadkillSection section = ConfigurationManager.GetSection("roadkill") as RoadkillSection;
			section.DataStoreType = "SQLite";
		}

		private void SetRegistry(RoadkillRegistry registry)
		{
			LocatorStartup.StartMVCInternal(registry, false);
		}

		[Test]
		public void Single_Constructor_Argument_Should_Register_Default_Instances()
		{
			// Arrange
			IocHelper.ConfigureLocator();

			// Act
			ApplicationSettings settings = LocatorStartup.Locator.GetInstance<ApplicationSettings>();
			IRepository repository = LocatorStartup.Locator.GetInstance<IRepository>();
			IUserContext context = LocatorStartup.Locator.GetInstance<IUserContext>();
			IPageService pageService = LocatorStartup.Locator.GetInstance<IPageService>();			
			MarkupConverter markupConverter = LocatorStartup.Locator.GetInstance<MarkupConverter>();
			CustomTokenParser tokenParser = LocatorStartup.Locator.GetInstance<CustomTokenParser>();
			UserViewModel userModel = LocatorStartup.Locator.GetInstance<UserViewModel>();
			SettingsViewModel settingsModel = LocatorStartup.Locator.GetInstance<SettingsViewModel>();
			AttachmentRouteHandler routerHandler = LocatorStartup.Locator.GetInstance<AttachmentRouteHandler>();
			UserServiceBase userManager = LocatorStartup.Locator.GetInstance<UserServiceBase>();
			IPluginFactory pluginFactory = LocatorStartup.Locator.GetInstance<IPluginFactory>();
			IWikiImporter wikiImporter = LocatorStartup.Locator.GetInstance<IWikiImporter>();
			IAuthorizationProvider authProvider = LocatorStartup.Locator.GetInstance<IAuthorizationProvider>();
			IActiveDirectoryProvider adProvider = LocatorStartup.Locator.GetInstance<IActiveDirectoryProvider>();


			// Assert
			Assert.That(settings, Is.Not.Null);
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
			IocHelper.ConfigureLocator();

			// Act
			IEnumerable<Roadkill.Core.Mvc.Controllers.ControllerBase> controllers = LocatorStartup.Locator.Container.GetAllInstances<Roadkill.Core.Mvc.Controllers.ControllerBase>();

			// Assert
			Assert.That(controllers.Count(), Is.GreaterThanOrEqualTo(9), LocatorStartup.Locator.Container.WhatDoIHave());
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
			IocHelper.ConfigureLocator(settings);

			// fake some AD settings for the AD service
			LocatorStartup.Locator.Container.Inject<ApplicationSettings>(settings);

			// Act
			IEnumerable<ServiceBase> services = LocatorStartup.Locator.GetAllInstances<ServiceBase>();

			// Assert
			Assert.That(services.Count(), Is.GreaterThanOrEqualTo(7));
		}

		[Test]
		public void Custom_Configuration_Repository_And_Context_Types_Should_Be_Registered()
		{
			// Arrange
			IocHelper.ConfigureLocator();

			// Act
			IRepository repository = LocatorStartup.Locator.GetInstance<IRepository>();
			IUserContext context = LocatorStartup.Locator.GetInstance<IUserContext>();

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
			IocHelper.ConfigureLocator(settings);

			// Act
			UserServiceBase usermanager = LocatorStartup.Locator.GetInstance<UserServiceBase>();

			// Assert
			Assert.That(usermanager, Is.TypeOf<ActiveDirectoryUserService>());
		}
#endif

		[Test]
		public void RegisterMvcFactoriesAndRouteHandlers_Should_Register_Instances()
		{
			// Arrange
			RouteTable.Routes.Clear();
			IocHelper.ConfigureLocator();

			// Act + Assert
			Assert.That(RouteTable.Routes.Count, Is.EqualTo(1));
			Assert.That(((Route)RouteTable.Routes[0]).RouteHandler, Is.TypeOf<AttachmentRouteHandler>());
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(SettingsViewModel)));
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(UserViewModel)));
		}

		[Test]
		public void Should_Load_Custom_Repository_From_DatabaseType()
		{
			// Arrange
			var settings = new ApplicationSettings();
			settings.DataStoreType = DataStoreType.MongoDB;
			IocHelper.ConfigureLocator(settings, false);

			// Act

			// Assert
			IRepository respository = LocatorStartup.Locator.GetInstance<IRepository>();
			Assert.That(respository, Is.TypeOf<MongoDBRepository>());
		}

		[Test]
		public void Should_Fill_ISetterInjected_Properties()
		{
			// Arrange
			IocHelper.ConfigureLocator();

			// Act
			ISetterInjected setterInjected = LocatorStartup.Locator.GetInstance<AdminRequiredAttribute>();

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
			IocHelper.ConfigureLocator();

			// Act
			IAuthorizationAttribute authorizationAttribute = LocatorStartup.Locator.GetInstance<AdminRequiredAttribute>();

			// Assert
			Assert.That(authorizationAttribute.AuthorizationProvider, Is.Not.Null);
		}

		[Test]
		public void Should_Copy_Plugins()
		{
			// TODO
		}

		[Test]
		public void Should_Load_Custom_UserService()
		{
			// Arrange
			ApplicationSettings applicationSettings = new ApplicationSettings();
			applicationSettings.UserServiceType = "Roadkill.Tests.UserServiceStub";
			IocHelper.ConfigureLocator(applicationSettings);

			// Put the UserServiceStub in a new assembly so we can test it's loaded
			string tempFilename = Path.GetFileName(Path.GetTempFileName()) + ".dll";
			string thisAssembly = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Roadkill.Tests.dll");
			string pluginSourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "UserService");
			string destPlugin = Path.Combine(pluginSourceDir, tempFilename);

			if (!Directory.Exists(pluginSourceDir))
				Directory.CreateDirectory(pluginSourceDir);

			File.Copy(thisAssembly, destPlugin, true);

			// Act + Assert
			UserServiceBase userManager = LocatorStartup.Locator.GetInstance<UserServiceBase>();
			Assert.That(userManager.GetType().FullName, Is.EqualTo("Roadkill.Tests.UserServiceStub"));
		}

		[Test]
		[Explicit]
		public void MongoDB_databaseType_Should_Load_Repository()
		{
			// Arrange
			var settings = new ApplicationSettings();
			settings.DataStoreType = DataStoreType.MongoDB;
			IocHelper.ConfigureLocator(settings);

			// Act

			// Assert
			Assert.That(LocatorStartup.Locator.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(FormsAuthUserService)));
		}

		[Test]
		public void Should_Use_FormsAuthUserService_By_Default()
		{
			// Arrange
			IocHelper.ConfigureLocator();

			// Act

			// Assert
			Assert.That(LocatorStartup.Locator.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(FormsAuthUserService)));
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
			IocHelper.ConfigureLocator(settings);

			// Act

			// Assert
			Assert.That(LocatorStartup.Locator.GetInstance<UserServiceBase>(), Is.TypeOf(typeof(ActiveDirectoryUserService)));
		}
#endif
	}
}
