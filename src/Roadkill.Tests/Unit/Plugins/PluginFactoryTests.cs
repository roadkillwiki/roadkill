using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.DependencyResolution.StructureMap;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;
using StructureMap;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class PluginFactoryTests
	{
		private static PluginFactory CreateFactory()
		{
			var container = new Container();
			return new PluginFactory(container);
		}

		[Test]
		public void copyassemblies_should_copy_all_dlls_to_pluginsbinpath()
		{
			// Arrange
			string sourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Text");
			CreatePluginFile(sourceDir, "Plugin1");
			CreatePluginFile(sourceDir, "Plugin2");
			CreatePluginFile(sourceDir, "Plugin3");

			string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			string plugin1Path = Path.Combine(destDir, "Plugin1", "copytextplugintest.dll");
			string plugin2Path = Path.Combine(destDir, "Plugin2", "copytextplugintest.dll");
			string plugin3Path = Path.Combine(destDir, "Plugin3", "copytextplugintest.dll");

			if (File.Exists(plugin1Path))
				File.Delete(plugin1Path);

			if (File.Exists(plugin2Path))
				File.Delete(plugin2Path);

			if (File.Exists(plugin3Path))
				File.Delete(plugin3Path);

			// Act
			PluginFactory.CopyAssemblies(sourceDir, destDir);

			// Assert
			Assert.That(File.Exists(plugin1Path), Is.True);
			Assert.That(File.Exists(plugin2Path), Is.True);
			Assert.That(File.Exists(plugin3Path), Is.True);
		}

		[Test]
		public void copyassemblies_should_copy_source_dll_when_source_is_more_recent()
		{
			// Arrange
			string pluginId = "PluginSourceMoreRecentTest";
			string sourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Text");
			string sourcePluginPath = Path.Combine(sourceDir, pluginId, "copytextplugintest.dll");
			CreatePluginFile(sourceDir, pluginId);

			string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			string pluginDestPath = Path.Combine(destDir, pluginId, "copytextplugintest.dll");
			CreatePluginFile(destDir, pluginId);

			Thread.Sleep(250); // slow the test down slightly
			File.WriteAllText(sourcePluginPath, "file has been updated"); // update the source plugin

			// Act
			PluginFactory.CopyAssemblies(sourceDir, destDir);

			// Assert
			string fileContent = File.ReadAllText(pluginDestPath);
			Assert.That(fileContent, Is.EqualTo("file has been updated"));
		}

		[Test]
		public void copyassemblies_should_not_copy_source_dll_when_destination_is_more_recent()
		{
			// Arrange
			string pluginId = "PluginDestMoreRecentTest";
			string sourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "Text");
			CreatePluginFile(sourceDir, pluginId);

			string destDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			string pluginDestFolder = Path.Combine(destDir, pluginId);
			string pluginDestPath = Path.Combine(pluginDestFolder, "copytextplugintest.dll");

			if (!Directory.Exists(pluginDestFolder))
				Directory.CreateDirectory(pluginDestFolder);
			File.WriteAllText(pluginDestPath, "dest file is more recent");  // create the plugin in the destination path so it's more recent

			// Act
			PluginFactory.CopyAssemblies(sourceDir, destDir);

			// Assert
			string fileContent = File.ReadAllText(pluginDestPath);
			Assert.That(fileContent, Is.EqualTo("dest file is more recent"));
		}

		/// <summary>
		/// Creates a DLL at "/Root/PluginId/copytextplugintest.dll". The dll is really a text file with the content provided.
		/// </summary>
		private void CreatePluginFile(string pluginRootDir, string pluginId)
		{
			string pluginFolder = Path.Combine(pluginRootDir, pluginId);

			if (!Directory.Exists(pluginFolder))
				Directory.CreateDirectory(pluginFolder);

			string dllPath = Path.Combine(pluginFolder, "copytextplugintest.dll");

			if (File.Exists(dllPath))
				File.Delete(dllPath);

			File.WriteAllText(dllPath, "");
		}

		[Test]
		public void gettextplugin_should_return_null_when_plugin_is_not_registered()
		{
			// Arrange
			PluginFactory factory = CreateFactory();

			// Act
			TextPlugin plugin = factory.GetTextPlugin("doesntexist");

			// Assert
			Assert.That(plugin, Is.Null);
		}

		[Test]
		public void registertextplugin_should_register_plugin_and_gettextplugin_should_return_plugin()
		{
			// Arrange
			PluginFactory factory = CreateFactory();
			TextPluginStub pluginStub = new TextPluginStub("randomid", "name", "desc");

			// Act
			factory.RegisterTextPlugin(pluginStub);
			TextPlugin actualPlugin = factory.GetTextPlugin("randomid");

			// Assert
			Assert.That(actualPlugin, Is.Not.Null);
		}

		[Test]
		public void gettextplugins_should_return_all_plugins()
		{
			// Arrange
			PluginFactory factory = CreateFactory();
			TextPluginStub plugin1Stub = new TextPluginStub("plugin1", "name", "desc");
			TextPluginStub plugin2Stub = new TextPluginStub("plugin2", "name", "desc");
			factory.RegisterTextPlugin(plugin1Stub);
			factory.RegisterTextPlugin(plugin2Stub);

			// Act
			IEnumerable<TextPlugin> allPlugins = factory.GetTextPlugins();

			// Assert
			Assert.That(allPlugins.Count(), Is.GreaterThanOrEqualTo(2));
		}
	}
}
