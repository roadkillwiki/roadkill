using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.Mvc.Controllers;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PluginSettingsControllerTests
	{
		[Test]
		public void Index_Should_Return_ViewResult_And_Model_With_2_PluginSummaries_Ordered_By_Name()
		{
			// Arrange
			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			pluginFactory.RegisterTextPlugin(new TextPluginStub("b id", "b name", "b desc"));
			pluginFactory.RegisterTextPlugin(new TextPluginStub("a id", "a name", "a desc"));
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory, null);

			// Act
			ViewResult result = controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			IEnumerable<PluginViewModel> pluginModels = result.ModelFromActionResult<IEnumerable<PluginViewModel>>();
			Assert.NotNull(pluginModels, "Null model");

			List<PluginViewModel> summaryList = pluginModels.ToList();

			Assert.That(summaryList.Count(), Is.EqualTo(2));
			Assert.That(summaryList[0].Name, Is.EqualTo("a name"));
			Assert.That(summaryList[1].Name, Is.EqualTo("b name"));
		}

		[Test]
		public void Edit_GET_Should_Return_ViewResult_And_Model_With_Known_Values()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();

			RepositoryMock repositoryMock = new RepositoryMock();
			repositoryMock.SaveTextPluginSettings(plugin);

			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			pluginFactory.RegisterTextPlugin(plugin);

			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory, repositoryMock);

			// Act
			ViewResult result = controller.Edit(plugin.Id) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			PluginViewModel model = result.ModelFromActionResult<PluginViewModel>();
			Assert.NotNull(model, "Null model");

			Assert.That(model.Id, Is.EqualTo(plugin.Id));
			Assert.That(model.Name, Is.EqualTo(plugin.Name));
			Assert.That(model.Description, Is.EqualTo(plugin.Description)); // ..full coverage TODO in PluginViewModelTests
		}

		[Test]
		public void Edit_GET_Should_Load_Settings_From_Repository()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();
			plugin.Settings.SetValue("name1", "default-value1");
			plugin.Settings.SetValue("name2", "default-value2");

			RepositoryMock repositoryMock = new RepositoryMock();
			repositoryMock.SaveTextPluginSettings(plugin);
			repositoryMock.TextPlugins[0].Settings.SetValue("name1", "value1");
			repositoryMock.TextPlugins[0].Settings.SetValue("name2", "value2");

			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			pluginFactory.RegisterTextPlugin(plugin);
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory, repositoryMock);

			// Act
			ViewResult result = controller.Edit(plugin.Id) as ViewResult;

			// Assert
			PluginViewModel model = result.ModelFromActionResult<PluginViewModel>();
			Assert.That(model.SettingValues[0].Value, Is.EqualTo("value1"));
			Assert.That(model.SettingValues[1].Value, Is.EqualTo("value2"));
		}

		[Test]
		public void Edit_GET_Should_Use_Default_Plugin_Settings_When_Plugin_Doesnt_Exist_In_Repository()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();
			plugin.Settings.SetValue("name1", "default-value1");
			plugin.Settings.SetValue("name2", "default-value2");

			RepositoryMock repositoryMock = new RepositoryMock();

			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			pluginFactory.RegisterTextPlugin(plugin);
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory, repositoryMock);

			// Act
			ViewResult result = controller.Edit(plugin.Id) as ViewResult;

			// Assert
			PluginViewModel model = result.ModelFromActionResult<PluginViewModel>();
			Assert.That(model.SettingValues[0].Value, Is.EqualTo("default-value1"));
			Assert.That(model.SettingValues[1].Value, Is.EqualTo("default-value2"));
		}

		[Test]
		public void Edit_GET_Should_Redirect_When_Id_Is_Empty()
		{
			// Arrange
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, null, null);

			// Act
			RedirectToRouteResult result = controller.Edit("") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Edit_GET_Should_Redirect_When_Plugin_Does_Not_Exist()
		{
			// Arrange
			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory, null);

			// Act
			RedirectToRouteResult result = controller.Edit("somepluginId") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Edit_POST_Should_Set_Setting_Values_From_Summary()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();
			plugin.Settings.SetValue("name1", "default-value1");
			plugin.Settings.SetValue("name2", "default-value2");

			RepositoryMock repositoryMock = new RepositoryMock();

			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			pluginFactory.RegisterTextPlugin(plugin);
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory, repositoryMock);

			PluginViewModel summary = new PluginViewModel();
			summary.Id = plugin.Id;
			summary.SettingValues = new List<SettingValue>();
			summary.SettingValues.Add(new SettingValue() { Name = "name1", Value = "new-value1" });
			summary.SettingValues.Add(new SettingValue() { Name = "name2", Value = "new-value2" });

			// Act
			ViewResult result = controller.Edit(summary) as ViewResult;

			// Assert
			List<SettingValue> values = repositoryMock.TextPlugins[0].Settings.Values.ToList();
			Assert.That(values[0].Value, Is.EqualTo("new-value1"));
			Assert.That(values[1].Value, Is.EqualTo("new-value2"));
		}

		[Test]
		public void Edit_POST_Should_Redirect_When_Plugin_Does_Not_Exist()
		{
			// Arrange
			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory, null);

			// Act
			RedirectToRouteResult result = controller.Edit("somepluginId") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}
	}
}
