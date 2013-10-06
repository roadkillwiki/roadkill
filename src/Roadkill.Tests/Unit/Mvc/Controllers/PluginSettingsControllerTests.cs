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
		public void Index_Should_Return_ViewResult_And_Model_With_2_PluginSummaries()
		{
			// Arrange
			PluginFactoryMock pluginFactory = new PluginFactoryMock();
			pluginFactory.RegisterTextPlugin(new TextPluginStub());
			pluginFactory.RegisterTextPlugin(new TextPluginStub());
			PluginSettingsController controller = new PluginSettingsController(null, null, null, null, pluginFactory);

			// Act
			ViewResult result = controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			IEnumerable<PluginSettingsSummary> summaries = result.ModelFromActionResult<IEnumerable<PluginSettingsSummary>>();
			Assert.NotNull(summaries, "Null model");
			Assert.That(summaries.Count(), Is.GreaterThanOrEqualTo(2));
		}
	}
}
