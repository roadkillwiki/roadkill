using NUnit.Framework;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	[TestFixture]
	[Category("Unit")]
	public class PluginViewModelTests
	{
		[Test]
		public void constructor_should_create_settingvalues()
		{
			// Arrange + Act
			PluginViewModel model = new PluginViewModel();	

			// Assert
			Assert.That(model.SettingValues, Is.Not.Null);
		}

		[Test]
		public void constructor_should_convert_textplugin_to_properties_and_description_newlines_to_br()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub("myid", "my name", "my description\r\nsome new text");
			plugin.Repository = new RepositoryMock();
			plugin.PluginCache = new SiteCache(CacheMock.RoadkillCache);
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
