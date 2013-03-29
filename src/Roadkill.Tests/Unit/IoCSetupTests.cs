using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Files;
using StructureMap;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	public class IoCSetupTests
	{
		[SetUp]
		public void Setup()
		{
			RoadkillSection section = ConfigurationManager.GetSection("roadkill") as RoadkillSection;
			section.DataStoreType = "SQLite";
		}

		[Test]
		public void NoConstructorArguments_Should_Register_Default_Instances()
		{
			// Arrange
			IoCSetup iocSetup = new IoCSetup();

			// Act
			iocSetup.Run();
			IConfigurationContainer config = ObjectFactory.GetInstance<IConfigurationContainer>();
			IRepository repository = ObjectFactory.GetInstance<IRepository>();
			IRoadkillContext context = ObjectFactory.GetInstance<IRoadkillContext>();
			IPageManager pageManager = ObjectFactory.GetInstance<IPageManager>();			
			MarkupConverter markupConverter = ObjectFactory.GetInstance<MarkupConverter>();
			CustomTokenParser tokenParser = ObjectFactory.GetInstance<CustomTokenParser>();
			UserSummary userSummary = ObjectFactory.GetInstance<UserSummary>();
			SettingsSummary settingsSummary = ObjectFactory.GetInstance<SettingsSummary>();
			AttachmentRouteHandler routerHandler = ObjectFactory.GetInstance<AttachmentRouteHandler>();
			UserManager userManager = ObjectFactory.GetInstance<UserManager>();

			// Assert
			Assert.That(config, Is.TypeOf<ConfigurationContainer>());
			Assert.That(repository, Is.TypeOf<LightSpeedRepository>());
			Assert.That(context, Is.TypeOf<RoadkillContext>());
			Assert.That(pageManager, Is.TypeOf<PageManager>());			
			Assert.That(markupConverter, Is.TypeOf<MarkupConverter>());
			Assert.That(tokenParser, Is.TypeOf<CustomTokenParser>());
			Assert.That(userSummary, Is.TypeOf<UserSummary>());
			Assert.That(settingsSummary, Is.TypeOf<SettingsSummary>());
			Assert.That(userManager, Is.TypeOf<DefaultUserManager>());
		}

		[Test]
		public void Should_Register_Controller_Instances()
		{
			// Arrange
			IoCSetup iocSetup = new IoCSetup();

			// Act
			iocSetup.Run();
			IList<Roadkill.Core.Controllers.ControllerBase> controllers = ObjectFactory.GetAllInstances<Roadkill.Core.Controllers.ControllerBase>();

			// Assert
			Assert.That(controllers.Count, Is.GreaterThanOrEqualTo(9));
		}

		[Test]
		public void Should_Register_Service_Instances()
		{
			// Arrange
			var config = new ConfigurationContainerStub();
			config.ApplicationSettings.UseWindowsAuthentication = true;
			config.ApplicationSettings.LdapConnectionString = "LDAP://123.com";
			config.ApplicationSettings.EditorRoleName = "editor;";
			config.ApplicationSettings.AdminRoleName = "admins";

			IoCSetup iocSetup = new IoCSetup();

			// Act
			iocSetup.Run();

			// fake some AD settings for the AD service
			ObjectFactory.Inject<IConfigurationContainer>(config);

			IList<ServiceBase> services = ObjectFactory.GetAllInstances<ServiceBase>();

			// Assert
			Assert.That(services.Count, Is.GreaterThanOrEqualTo(7));
		}

		[Test]
		public void Custom_Configuration_Repository_And_Context_Types_Should_Be_Registered()
		{
			// Arrange
			IoCSetup iocSetup = new IoCSetup(new ConfigurationContainerStub(), new RepositoryStub(), new RoadkillContextStub());

			// Act
			iocSetup.Run();
			IConfigurationContainer container = ObjectFactory.GetInstance<IConfigurationContainer>();
			IRepository repository = ObjectFactory.GetInstance<IRepository>();
			IRoadkillContext context = ObjectFactory.GetInstance<IRoadkillContext>();

			// Assert
			Assert.That(container, Is.TypeOf<ConfigurationContainerStub>());
			Assert.That(repository, Is.TypeOf<RepositoryStub>());
			Assert.That(context, Is.TypeOf<RoadkillContextStub>());
		}

		[Test]
		public void WindowsAuth_Should_Register_ActiveDirectoryUserManager()
		{
			// Arrange
			var config = new ConfigurationContainerStub();
			config.ApplicationSettings.UseWindowsAuthentication = true;
			config.ApplicationSettings.LdapConnectionString = "LDAP://123.com";
			config.ApplicationSettings.EditorRoleName = "editor;";
			config.ApplicationSettings.AdminRoleName = "admins";

			IoCSetup iocSetup = new IoCSetup(config, new RepositoryStub(), new RoadkillContextStub());

			// Act
			iocSetup.Run();
			UserManager usermanager = ObjectFactory.GetInstance<UserManager>();

			// Assert
			Assert.That(usermanager, Is.TypeOf<ActiveDirectoryUserManager>());
		}

		[Test]
		public void RegisterMvcFactoriesAndRouteHandlers_Should_Register_Instances()
		{
			// Arrange
			IoCSetup iocSetup = new IoCSetup();

			// Act
			iocSetup.Run();
			iocSetup.RegisterMvcFactoriesAndRouteHandlers();

			// Assert
			Assert.That(RouteTable.Routes.Count, Is.EqualTo(1));
			Assert.That(((Route)RouteTable.Routes[0]).RouteHandler, Is.TypeOf<AttachmentRouteHandler>());
			Assert.That(ControllerBuilder.Current.GetControllerFactory(), Is.TypeOf<ControllerFactory>());
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(SettingsSummary)));
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(UserSummary)));
		}

		[Test]
		[ExpectedException(typeof(IoCException))]
		public void RegisterMvcFactoriesAndRouteHandlers_Requires_Run_First()
		{
			// Arrange
			IoCSetup iocSetup = new IoCSetup();

			// Act
			iocSetup.RegisterMvcFactoriesAndRouteHandlers();

			// Assert
		}

		[Test]
		public void Should_Load_Custom_Repository_From_DatabaseType()
		{
			// Arrange
			IoCSetup iocSetup = new IoCSetup();
			RoadkillSection section = ConfigurationManager.GetSection("roadkill") as RoadkillSection;
			section.DataStoreType = "MongoDB";

			// Act
			iocSetup.Run();

			// Assert
			IRepository respository = ObjectFactory.GetInstance<IRepository>();
			Assert.That(respository, Is.TypeOf<MongoDBRepository>());
		}

		[Test]
		public void Should_Load_Custom_UserManager()
		{
			// Arrange
			IoCSetup iocSetup = new IoCSetup();
			RoadkillSection section = ConfigurationManager.GetSection("roadkill") as RoadkillSection;
			section.UserManagerType = "Roadkill.Tests.UserManagerStub";

			string sourcePlugin = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Roadkill.Tests.dll");
			string pluginDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Plugins");
			string destPlugin = Path.Combine(pluginDir, "Roadkill.Tests2.dll");

			if (!Directory.Exists(pluginDir))
				Directory.CreateDirectory(pluginDir);

			File.Copy(sourcePlugin, destPlugin, true);

			// Act
			iocSetup.Run();

			// Assert
			UserManager userManager = ObjectFactory.GetInstance<UserManager>();
			Assert.That(userManager.GetType().FullName, Is.EqualTo("Roadkill.Tests.UserManagerStub"));
		}
	}
}
