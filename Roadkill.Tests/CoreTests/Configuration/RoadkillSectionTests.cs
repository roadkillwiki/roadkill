using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Converters;
using Roadkill.Tests.Integration;

namespace Roadkill.Tests.CoreTests
{
	[TestFixture]
	[Description("These tests are for the web.config Section.")]
	public class RoadkillSectionTests
	{
		[Test]
		[RequiresSTA] 
		public void RoadkillSection_Properties_Have_Correct_Key_Mappings_And_Values()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test.config");

			// Act
			RoadkillSection.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(RoadkillSection.Current.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName");
			Assert.That(RoadkillSection.Current.AttachmentsFolder, Is.EqualTo("/Attachments-test"), "AttachmentsFolder");
			Assert.That(RoadkillSection.Current.CacheEnabled, Is.True, "CacheEnabled");
			Assert.That(RoadkillSection.Current.CacheText, Is.True, "CacheText");
			Assert.That(RoadkillSection.Current.ConnectionStringName, Is.EqualTo("Roadkill-test"), "ConnectionStringName");
			Assert.That(RoadkillSection.Current.DatabaseType, Is.EqualTo("SQLite"), "DatabaseType");
			Assert.That(RoadkillSection.Current.EditorRoleName, Is.EqualTo("Editor-test"), "EditorRoleName");
			Assert.That(RoadkillSection.Current.IgnoreSearchIndexErrors, Is.True, "IgnoreSearchIndexErrors");
			Assert.That(RoadkillSection.Current.Installed, Is.True, "Installed");
			Assert.That(RoadkillSection.Current.IsPublicSite, Is.False, "IsPublicSite");
			Assert.That(RoadkillSection.Current.LdapConnectionString, Is.EqualTo("ldapstring-test"), "LdapConnectionString");
			Assert.That(RoadkillSection.Current.LdapPassword, Is.EqualTo("ldappassword-test"), "LdapPassword");
			Assert.That(RoadkillSection.Current.LdapUsername, Is.EqualTo("ldapusername-test"), "LdapUsername");
			Assert.That(RoadkillSection.Current.ResizeImages, Is.True, "ResizeImages");
			Assert.That(RoadkillSection.Current.UserManagerType, Is.EqualTo("SqlUserManager-test"), "SqlUserManager");
			Assert.That(RoadkillSection.Current.UseWindowsAuthentication, Is.False, "UseWindowsAuthentication");
		}

		[Test]
		[RequiresSTA]
		public void RoadkillSection_Optional_Settings_With_Missing_Values_Have_Default_Values()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test-optional-values.config");

			// Act
			RoadkillSection.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(RoadkillSection.Current.DatabaseType, Is.EqualTo(""), "DatabaseType");
			Assert.That(RoadkillSection.Current.IgnoreSearchIndexErrors, Is.False, "IgnoreSearchIndexErrors");
			Assert.That(RoadkillSection.Current.IsPublicSite, Is.True, "IsPublicSite");
			Assert.That(RoadkillSection.Current.LdapConnectionString, Is.EqualTo(""), "LdapConnectionString");
			Assert.That(RoadkillSection.Current.LdapPassword, Is.EqualTo(""), "LdapPassword");
			Assert.That(RoadkillSection.Current.LdapUsername, Is.EqualTo(""), "LdapUsername");
			Assert.That(RoadkillSection.Current.ResizeImages, Is.True, "ResizeImages");
			Assert.That(RoadkillSection.Current.UserManagerType, Is.EqualTo(""), "SqlUserManager");
		}

		[Test]
		[RequiresSTA]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void RoadkillSection_Missing_Values_Throw_Exception()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestConfigs", "test-missing-values.config");

			// Act
			RoadkillSection.LoadCustomConfigFile(configFilePath);

			// Assert (call the Current method to load the config)
			var x = RoadkillSection.Current.ConnectionStringName;
		}
	}
}