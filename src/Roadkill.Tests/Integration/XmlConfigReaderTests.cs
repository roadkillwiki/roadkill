using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;

namespace Roadkill.Tests.Integration
{
	[TestFixture]
	[Category("Integration")]
	public class XmlConfigReaderTests
	{
		[Test]
		[ExpectedException(typeof(ConfigurationException))]
		public void Constructor_Should_Throw_Exception_If_Does_Not_Exist()
		{
			// Arrange
			string configFile = GetConfigPath("doesntexist.config");

			// Act + Assert
			XmlConfigReader reader = new XmlConfigReader(configFile);
		}

		[Test]
		[ExpectedException(typeof(ConfigurationException))]
		public void Read_Should_Throw_Exception_If_No_Roadkill_Section_Exists()
		{
			// Arrange
			string configFile = GetConfigPath("test-no-roadkillsection.config");
			XmlConfigReader reader = new XmlConfigReader(configFile);

			// Act
			RoadkillSection section = reader.ReadSection();

			// Assert
		}

		[Test]
		public void Read_Should_Return_Section_With_Same_Values_As_Config_File()
		{
			// Arrange
			string configFile = GetConfigPath("test.config");
			XmlConfigReader reader = new XmlConfigReader(configFile);

			// Act
			RoadkillSection section = reader.ReadSection();

			// Assert
			Assert.That(section.AdminRoleName, Is.EqualTo("Admin-test"));
			Assert.That(section.AttachmentsFolder, Is.EqualTo("/Attachments-test"));
			Assert.That(section.AttachmentsRoutePath, Is.EqualTo("AttachmentsRoutePathTest"));
			Assert.That(section.UseObjectCache, Is.True);
			Assert.That(section.UseBrowserCache, Is.True);
			Assert.That(section.ConnectionStringName, Is.EqualTo("Roadkill-test"));
			Assert.That(section.DataStoreType, Is.EqualTo("SQLite"));
			Assert.That(section.EditorRoleName, Is.EqualTo("Editor-test"));
			Assert.That(section.IgnoreSearchIndexErrors, Is.True);
			Assert.That(section.Installed, Is.True);
			Assert.That(section.IsPublicSite, Is.False);
			Assert.That(section.LdapConnectionString, Is.EqualTo("ldapstring-test"));
			Assert.That(section.LdapUsername, Is.EqualTo("ldapusername-test"));
			Assert.That(section.LdapPassword, Is.EqualTo("ldappassword-test"));
			Assert.That(section.Logging, Is.EqualTo("All"));
			Assert.That(section.LogErrorsOnly, Is.False);
			Assert.That(section.ResizeImages, Is.True);
			Assert.That(section.RepositoryType, Is.EqualTo("Repository-test"));
			Assert.That(section.UserManagerType, Is.EqualTo("DefaultUserManager-test"));
			Assert.That(section.UseHtmlWhiteList, Is.False);
			Assert.That(section.UseWindowsAuthentication, Is.False);
			Assert.That(section.Version, Is.EqualTo("1.8.0"));
		}

		private string GetConfigPath(string filename)
		{
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Integration", "TestConfigs", filename);
		}
	}
}
