using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PluginViewModelTests
	{
		[Test]
		public void Constructor_Should_Create_SettingValues()
		{
			// Arrange + Act
			PluginViewModel model = new PluginViewModel();	

			// Assert
			Assert.That(model.SettingValues, Is.Not.Null);
		}

		[Test]
		public void Constructor_Should_Convert_TextPlugin_To_Properties_And_Description_Newlines_To_Br()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub("myid", "my name", "my description\r\nsome new text");
			plugin.Repository = new RepositoryMock();
			plugin.PluginCache = new SiteCache(new ApplicationSettings(), CacheMock.RoadkillCache);
			plugin.Settings.IsEnabled = true;

			// Act
			PluginViewModel model = new PluginViewModel(plugin);

			// Assert
			Assert.That(model.SettingValues, Is.Not.Null);
			Assert.That(model.Id, Is.EqualTo(plugin.Id));
			Assert.That(model.Name, Is.EqualTo(plugin.Name));
			Assert.That(model.IsEnabled, Is.True);
			Assert.That(model.DatabaseId, Is.EqualTo(plugin.DatabaseId));
			Assert.That(model.Description, Is.EqualTo("my description<br/><br/>some new text"));
		}
	}
}
