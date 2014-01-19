using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;
using PluginSettings = Roadkill.Core.Plugins.Settings;

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
			TextPluginStub plugin1 = new TextPluginStub("PluginId1", "name", "desc");
			TextPluginStub plugin2 = new TextPluginStub("PluginId2", "name", "desc");

			// Act
			Guid id1 = plugin1.DatabaseId;
			Guid id2 = plugin2.DatabaseId;

			// Assert
			Assert.That(id1, Is.Not.EqualTo(id2));
		}

		[Test]
		public void PluginVirtualPath_Should_Contain_Plugin_Id_And_No_Trailing_Slash()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub("Plugin1", "name", "desc");

			// Act
			string virtualPath = plugin.PluginVirtualPath;

			// Assert
			Assert.That(virtualPath, Is.StringContaining("Plugin1"));
			Assert.That(virtualPath, Is.StringStarting("~/Plugins/"));
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
		[ExpectedException(typeof(PluginException))]
		public void DatabaseId_Should_Default_Version_To_1_If_Version_Is_Empty()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub("", "", "", "");

			// Act + Assert
			PluginSettings settings = plugin.Settings;
		}

		[Test]
		public void Constructor_Should_Set_Cacheable_To_True()
		{
			// Arrange
			RepositoryMock repository = new RepositoryMock();
			ApplicationSettings appSettings = new ApplicationSettings();

			TextPluginStub plugin = new TextPluginStub();

			// Act + Assert
			Assert.That(plugin.IsCacheable, Is.True);
		}

		[Test]
		public void Settings_Should_Not_Be_Null()
		{
			// Arrange + act
			TextPlugin plugin = new TextPluginStub();
			plugin.Repository = new RepositoryMock();
			plugin.PluginCache = new SiteCache(new ApplicationSettings(), CacheMock.RoadkillCache);

			// Assert
			Assert.That(plugin.Settings, Is.Not.Null);
		}

		[Test]
		public void GetSettingsJson_Should_Return_Expected_Json()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = new RepositoryMock();
			plugin.PluginCache = new SiteCache(new ApplicationSettings(), CacheMock.RoadkillCache);

			plugin.Settings.SetValue("setting1", "value1");
			plugin.Settings.SetValue("setting2", "value2");
			plugin.Settings.IsEnabled = true;

			string expectedJson = @"{
  ""PluginId"": ""Amazing plugin"",
  ""Version"": ""1.0"",
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
			TextPlugin plugin = new TextPluginStub();
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
		public void SetHeadJsOnLoadedFunction_Should_Be_Added_To_Javascript()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();
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
		public void GetCssLink_Should_Contain_File_And_Expected_Html()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub("PluginId", "name", "desc");
			string expectedHtml = "\t\t" +
								 @"<link href=""~/Plugins/PluginId/file.css?version={PluginVersion}"" rel=""stylesheet"" type=""text/css"" />" +
								 "\n";

			expectedHtml = expectedHtml.Replace("{PluginVersion}", plugin.Version);

			// Act
			string actualHtml = plugin.GetCssLink("file.css");

			// Assert
			Assert.That(actualHtml, Is.EqualTo(expectedHtml), actualHtml);
		}

		[Test]
		[Description("The SiteCache property should be injected at creation time by Structuremap")]
		[ExpectedException(typeof(PluginException))]
		public void Settings_Should_Throw_Exception_If_SiteCache_Is_Not_Set()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();

			// Act + Assert
			PluginSettings settings = plugin.Settings;
		}

		[Test]
		[Description("The Repository property should be injected at creation time by Structuremap")]
		[ExpectedException(typeof(PluginException))]
		public void Settings_Should_Throw_Exception_If_Repository_Is_Not_Set()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = new SiteCache(null, new CacheMock());

			// Act + Assert
			PluginSettings settings = plugin.Settings;
		}

		[Test]
		public void Settings_Should_Return_Member_Instance_On_Second_Call_And_Not_Load_From_Cache_Or_Repository()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings appSettings = new ApplicationSettings();
			Mock<IPluginCache> pluginCacheMock = new Mock<IPluginCache>();
			Mock<IRepository> mockRepository = new Mock<IRepository>();

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = pluginCacheMock.Object;
			plugin.Repository = mockRepository.Object;

			// Act
			PluginSettings settings = plugin.Settings;
			settings = plugin.Settings;

			// Assert
			Assert.That(settings, Is.Not.Null);
			pluginCacheMock.Verify(x => x.GetPluginSettings(plugin), Times.Once); // 1st time only
			pluginCacheMock.Verify(x => x.UpdatePluginSettings(plugin), Times.Once);
			mockRepository.Verify(x => x.GetTextPluginSettings(plugin.DatabaseId), Times.Once);
			mockRepository.Verify(x => x.SaveTextPluginSettings(plugin), Times.Once);
		}

		[Test]
		public void Settings_Should_Load_From_Cache_When_Settings_Exist_In_Cache()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings appSettings = new ApplicationSettings();
			Mock<IPluginCache> pluginCacheMock = new Mock<IPluginCache>();
			PluginSettings expectedPluginSettings = new PluginSettings("mockplugin", "1.0");
			expectedPluginSettings.SetValue("cache", "test");

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = pluginCacheMock.Object;

			pluginCacheMock.Setup(x => x.GetPluginSettings(plugin)).Returns(expectedPluginSettings);

			// Act
			PluginSettings actualPluginSettings = plugin.Settings;

			// Assert
			Assert.That(actualPluginSettings, Is.Not.Null);
			Assert.That(actualPluginSettings.GetValue("cache"), Is.EqualTo("test"));
		}

		[Test]
		public void Settings_Should_Load_From_Repository_When_Cache_Is_Not_Set()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings appSettings = new ApplicationSettings();
			Mock<IPluginCache> pluginCacheMock = new Mock<IPluginCache>();

			PluginSettings expectedPluginSettings = new PluginSettings("mockplugin", "1.0");
			expectedPluginSettings.SetValue("repository", "test");
			RepositoryMock repository = new RepositoryMock();
			repository.PluginSettings = expectedPluginSettings;

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = pluginCacheMock.Object;
			plugin.Repository = repository;

			// Act
			PluginSettings actualPluginSettings = plugin.Settings;

			// Assert
			Assert.That(actualPluginSettings, Is.Not.Null);
			Assert.That(actualPluginSettings.GetValue("repository"), Is.EqualTo("test"));
		}

		[Test]
		public void Settings_Should_Create_Instance_When_Repository_Has_No_Settings()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings appSettings = new ApplicationSettings();
			Mock<IPluginCache> pluginCacheMock = new Mock<IPluginCache>();
			RepositoryMock repository = new RepositoryMock();

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = pluginCacheMock.Object;
			plugin.Repository = repository;

			// Act
			PluginSettings actualPluginSettings = plugin.Settings;

			// Assert
			Assert.That(actualPluginSettings, Is.Not.Null);
			Assert.That(actualPluginSettings.Values.Count(), Is.EqualTo(0));
		}

		[Test]
		[ExpectedException(typeof(PluginException))]
		public void Settings_Should_Throw_Exception_If_Id_Is_Not_Set()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings appSettings = new ApplicationSettings();
			Mock<IPluginCache> pluginCacheMock = new Mock<IPluginCache>();
			RepositoryMock repository = new RepositoryMock();

			TextPluginStub plugin = new TextPluginStub("","","","");
			plugin.PluginCache = pluginCacheMock.Object;
			plugin.Repository = repository;

			// Act + Assert
			PluginSettings actualPluginSettings = plugin.Settings;
		}

		[Test]
		public void Settings_Should_Default_Version_To_1_If_Version_Is_Empty()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings appSettings = new ApplicationSettings();
			Mock<IPluginCache> pluginCacheMock = new Mock<IPluginCache>();
			RepositoryMock repository = new RepositoryMock();

			TextPluginStub plugin = new TextPluginStub("id", "name", "desc", "");
			plugin.PluginCache = pluginCacheMock.Object;
			plugin.Repository = repository;

			// Act
			PluginSettings actualPluginSettings = plugin.Settings;

			// Assert
			Assert.That(actualPluginSettings.Version, Is.EqualTo("1.0"));
		}

		[Test]
		public void Settings_Should_Call_OnInitializeSettings_When_Repository_Has_No_Settings()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings appSettings = new ApplicationSettings();
			Mock<IPluginCache> pluginCacheMock = new Mock<IPluginCache>();
			RepositoryMock repository = new RepositoryMock();

			Mock<TextPluginStub> pluginMock = new Mock<TextPluginStub>();
			pluginMock.Setup(x => x.Id).Returns("SomeId");
			pluginMock.Object.PluginCache = pluginCacheMock.Object;
			pluginMock.Object.Repository = repository;

			// Act
			PluginSettings actualPluginSettings = pluginMock.Object.Settings;

			// Assert
			Assert.That(actualPluginSettings, Is.Not.Null);
			pluginMock.Verify(x => x.OnInitializeSettings(It.IsAny<PluginSettings>()), Times.Once);
		}

		[Test]
		public void Settings_Should_Save_To_Repository_When_Repository_Has_No_Settings()
		{
			// Arrange
			CacheMock cache = new CacheMock();
			ApplicationSettings appSettings = new ApplicationSettings();
			Mock<IPluginCache> pluginCacheMock = new Mock<IPluginCache>();
			RepositoryMock repository = new RepositoryMock();

			TextPluginStub plugin = new TextPluginStub();
			plugin.PluginCache = pluginCacheMock.Object;
			plugin.Repository = repository;

			// Act
			PluginSettings actualPluginSettings = plugin.Settings;

			// Assert
			Assert.That(actualPluginSettings, Is.Not.Null);
			Assert.That(repository.TextPlugins.Count, Is.EqualTo(1));
			Assert.That(repository.TextPlugins.FirstOrDefault(), Is.EqualTo(plugin));
		}
	}
}