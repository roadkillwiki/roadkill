using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
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
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory);

			// Act
			ViewResult result = controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			IEnumerable<PluginSummary> summaries = result.ModelFromActionResult<IEnumerable<PluginSummary>>();
			Assert.NotNull(summaries, "Null model");

			List<PluginSummary> summaryList = summaries.ToList();

			Assert.That(summaryList.Count(), Is.EqualTo(2));
			Assert.That(summaryList[0].Name, Is.EqualTo("a name"));
			Assert.That(summaryList[1].Name, Is.EqualTo("b name"));
		}

		[Test]
		public void Edit_GET_Should_Return_ViewResult_And_Model_With_Known_Values()
		{
			// Arrange
			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			TextPluginStub plugin = new TextPluginStub();
			pluginFactory.RegisterTextPlugin(plugin);
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory);

			// Act
			ViewResult result = controller.Edit(plugin.Id) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			PluginSummary summary = result.ModelFromActionResult<PluginSummary>();
			Assert.NotNull(summary, "Null model");

			Assert.That(summary.Id, Is.EqualTo(plugin.Id));
			Assert.That(summary.Name, Is.EqualTo(plugin.Name));
			Assert.That(summary.Description, Is.EqualTo(plugin.Description)); // ..full coverage in PluginSummary tests
		}
	}
}
