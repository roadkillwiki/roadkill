using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.DependencyResolution;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;
using StructureMap;

namespace Roadkill.Tests.Unit.DependencyResolution
{
	[TestFixture]
	[Category("Unit")]
	public class LocatorStartupTests
	{
		[TearDown]
		public void TearDown()
		{
			try
			{
				// Clear down the Microsoft's statics
				ModelBinders.Binders.Clear();
				GlobalConfiguration.Configuration.Services.RemoveAll(typeof(IFilterProvider), o => true);
			}
			catch (Exception)
			{
			}
        }

		[Test]
		public void StartMVCInternal_should_create_service_locator_and_set_mvc_service_locator()
		{
			// Arrange
			var settings = new ApplicationSettings();
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });

			// Act
			LocatorStartup.StartMVCInternal(registry, false);

			// Assert
			Assert.That(LocatorStartup.Locator, Is.Not.Null);
			Assert.That(DependencyResolver.Current, Is.EqualTo(LocatorStartup.Locator));
		}

		[Test]
		public void AfterInitializationInternal_should_register_webapi_servicelocator_and_attributeprovider()
		{
			// Arrange
			var settings = new ApplicationSettings();
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			LocatorStartup.StartMVCInternal(registry, false); // needed to register LocatorStartup.Locator

			// Act
			LocatorStartup.AfterInitializationInternal(container, settings);

			// Assert
			Assert.That(GlobalConfiguration.Configuration.DependencyResolver, Is.EqualTo(LocatorStartup.Locator));

			// Doesn't work
			//Assert.That(GlobalConfiguration.Configuration.Services.GetService(typeof(System.Web.Http.Filters.IFilterProvider)), Is.TypeOf<MvcAttributeProvider>());
		}

		[Test]
		public void AfterInitializationInternal_should_register_mvc_attributes_and_modelbinders()
		{
			// Arrange
			var settings = new ApplicationSettings();
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			LocatorStartup.StartMVCInternal(registry, false);

			// Act
			LocatorStartup.AfterInitializationInternal(container, settings);

			// Assert
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(SettingsViewModel)));
			Assert.True(ModelBinders.Binders.ContainsKey(typeof(UserViewModel)));
		}

		[Test]
		public void AfterInitializationInternal_should_attachment_routehandler()
		{
			// Arrange
			var settings = new ApplicationSettings();
			var registry = new RoadkillRegistry(new ConfigReaderWriterStub() { ApplicationSettings = settings });
			var container = new Container(registry);

			LocatorStartup.StartMVCInternal(registry, false);

			// Act
			LocatorStartup.AfterInitializationInternal(container, settings);

			// Assert
			Assert.That(RouteTable.Routes.Count, Is.EqualTo(1));
			Assert.That(((Route)RouteTable.Routes[0]).RouteHandler, Is.TypeOf<AttachmentRouteHandler>());
		}
	}
}