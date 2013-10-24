using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class TextPluginTests
	{
		[Test]
		public void DatabaseId_Should_Be_Different_Based_On_Id()
		{
			// Arrange
			Mock<TextPlugin> mock1 = new Mock<TextPlugin>(null, null);
			mock1.SetupAllProperties();
			mock1.Setup(x => x.Id).Returns("Plugin1");
			mock1.Setup(x => x.Name).Returns("My Plugin");

			Mock<TextPlugin> mock2 = new Mock<TextPlugin>(null, null);
			mock2.SetupAllProperties();
			mock2.Setup(x => x.Id).Returns("Plugin2");
			mock2.Setup(x => x.Name).Returns("My Plugin");

			// Act
			Guid id1 = mock1.Object.DatabaseId;
			Guid id2 = mock2.Object.DatabaseId;

			// Assert
			Assert.That(id1, Is.Not.EqualTo(id2));
		}

		[Test]
		public void PluginVirtualPath_Should_Contain_Plugin_Id_And_No_Trailing_Slash()
		{
			// Arrange
			Mock<TextPlugin> mock1 = new Mock<TextPlugin>(null, null);
			mock1.SetupAllProperties();
			mock1.Setup(x => x.Id).Returns("Plugin1");
			mock1.Setup(x => x.Name).Returns("My Plugin");

			// Act
			string virtualPath = mock1.Object.PluginVirtualPath;

			// Assert
			Assert.That(virtualPath, Is.StringContaining("Plugin1"));
			Assert.That(virtualPath, Is.StringStarting("~/Plugins/Text/"));
			Assert.That(virtualPath, Is.Not.StringEnding("/"));
		}

		[Test]
		[ExpectedException(typeof(PluginException))]
		public void PluginVirtualPath_Should_Throw_Exception_When_Id_Is_Empty()
		{
			// Arrange
			string id = "";
			TextPluginStub plugin = new TextPluginStub(id, "name", "description");

			// Act + Assert
			string path = plugin.PluginVirtualPath;
		}

		[Test]
		[ExpectedException(typeof(PluginException))]
		public void DatabaseId_Should_Throw_Exception_When_Id_Is_Empty()
		{
			// Arrange
			string id = "";
			TextPluginStub plugin = new TextPluginStub(id, "name", "description");

			// Act + Assert
			Guid databaseId = plugin.DatabaseId;
		}

		[Test]
		public void Constructor_Should_Create_Settings_And_Set_Properties_From_Parameters()	
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			ApplicationSettings appSettings = new ApplicationSettings();

			TextPlugin plugin = new Mock<TextPlugin>(appSettings, repository).Object;

			// Act + Assert
			Assert.That(plugin.ApplicationSettings, Is.EqualTo(appSettings));
			Assert.That(plugin.SiteSettings, Is.EqualTo(repository.SiteSettings));
			Assert.That(plugin.Settings, Is.Not.Null);
		}

		[Test]
		public void Settings_Should_Never_Be_Null()
		{
			// Arrange + act
			TextPlugin plugin = new Mock<TextPlugin>(null, null).Object;

			// Assert
			Assert.That(plugin.Settings, Is.Not.Null);
		}

		[Test]
		public void GetSettingsJson_Should_Return_Expected_Json()
		{
			// Arrange
			TextPlugin plugin = new Mock<TextPlugin>(null, null).Object;
			plugin.Settings.SetValue("setting1", "value1");
			plugin.Settings.SetValue("setting2", "value2");
			plugin.Settings.IsEnabled = true;

			string expectedJson = @"{
  ""IsEnabled"": true,
  ""Values"": [
    {
      ""Name"": ""setting1"",
      ""Value"": ""value1"",
      ""FormType"": 0
    },
    {
      ""Name"": ""setting2"",
      ""Value"": ""value2"",
      ""FormType"": 0
    }
  ]
}";

			// Act
			string actualJson = plugin.GetSettingsJson();

			// Assert
			Console.WriteLine(actualJson);
			Assert.That(actualJson, Is.EqualTo(expectedJson));
		}

		[Test]
		public void GetJavascriptHtml_Should_Contain_Scripts_With_HeadJs()
		{
			// Arrange
			TextPlugin plugin = new Mock<TextPlugin>(null, null).Object;
			plugin.AddScript("pluginscript.js", "script1");
			string expectedHtml = @"<script type=""text/javascript"">" +
								@"head.js({ ""script1"", ""pluginscript.js"" },function() {  })" +
								"</script>\n";

			// Act
			string actualHtml = plugin.GetJavascriptHtml();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void HeadJsLoadedFunction_Should_Be_Added_To_Javascript()
		{
			// Arrange
			TextPlugin plugin = new Mock<TextPlugin>(null, null).Object;
			plugin.AddScript("pluginscript.js", "script1");
			plugin.SetHeadJsOnLoadedFunction("alert('done')");
			
			string expectedHtml = @"<script type=""text/javascript"">" +
								@"head.js({ ""script1"", ""pluginscript.js"" },function() { alert('done') })" +
								"</script>\n";

			// Act
			string actualHtml = plugin.GetJavascriptHtml();

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		public void GetCss_Should_Contain_File_And_Expected_Html()
		{
			// Arrange
			Mock<TextPlugin> plugin = new Mock<TextPlugin>(null, null);
			plugin.Setup(x => x.Id).Returns("PluginId");
			string expectedHtml = "\t\t" +
								 @"<link href=""~/Plugins/Text/PluginId/file.css"" rel=""stylesheet"" type=""text/css"" />" +
								 "\n";

			// Act
			string actualHtml = plugin.Object.GetCssLink("file.css");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}
	}
}