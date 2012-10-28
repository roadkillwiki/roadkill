using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Converters;

namespace Roadkill.Tests.CoreTests
{
	[TestFixture]
	[Description("White box tests for the web.config section parser")]
	[Explicit("Run these tests in order, so multi-threaded runs don't interfere with each other.")]
	public class RoadkillSectionTests
	{
		[Test]
		public void Properties_Have_Correct_Key_Mappings_And_Values()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test.config");
					
			// Act
			RoadkillSection.LoadCustomConfigFile(configFilePath);

			// Assert
			Assert.That(RoadkillSection.Current.AdminRoleName, Is.EqualTo("Admin-test"), "AdminRoleName");
			Assert.That(RoadkillSection.Current.AttachmentsFolder, Is.EqualTo("~/Attachments-test"), "AttachmentsFolder");
			Assert.That(RoadkillSection.Current.CacheEnabled, Is.True, "CacheEnabled");
			Assert.That(RoadkillSection.Current.CacheText, Is.True, "CacheText");
			Assert.That(RoadkillSection.Current.ConnectionStringName, Is.EqualTo("Roadkill-test"), "ConnectionStringName");
			Assert.That(RoadkillSection.Current.DatabaseType, Is.EqualTo("SQLite-test"), "DatabaseType");
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
		public void Optional_Properties_With_Missing_Values_Have_Default_Values()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-optional-values.config");

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
		[RequiresMTA]
		[ExpectedException(typeof(ConfigurationErrorsException))]
		public void Missing_Values_Throw_Exception()
		{
			// Arrange
			string configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "test-missing-values.config");

			// Act
			RoadkillSection.LoadCustomConfigFile(configFilePath);

			// Assert (call the Current method to load the config)
			var x = RoadkillSection.Current.ConnectionStringName;
			Assert.That(RoadkillSection.Current.ConnectionStringName, Is.EqualTo(""), "ConnectionStringName");
		}
	}
}