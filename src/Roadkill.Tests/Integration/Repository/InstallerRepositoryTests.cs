using NUnit.Framework;
using Roadkill.Core.Database;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Integration")]
	public abstract class InstallerRepositoryTests
	{
		protected abstract string InvalidConnectionString { get; }
		protected abstract string ConnectionString { get; }

		protected IInstallerRepository Repository;
		protected abstract IInstallerRepository GetRepository();
		protected abstract void Clearup();
		protected abstract void CheckDatabaseProcessIsRunning();

		[SetUp]
		public void Setup()
		{
			// Setup the repository
			Repository = GetRepository();
			Clearup();
		}

		[TearDown]
		public void TearDown()
		{
			Repository.Dispose();
		}

		[Test]
		public void install_should()
		{
			//// Arrange

			//// Act
			//Repository.Install(ApplicationSettings.DatabaseName, ApplicationSettings.ConnectionString, false);

			//// Assert
			//Assert.That(Repository.AllPages().Count(), Is.EqualTo(0));
			//Assert.That(Repository.AllPageContents().Count(), Is.EqualTo(0));
			//Assert.That(Repository.FindAllAdmins().Count(), Is.EqualTo(0));
			//Assert.That(Repository.FindAllEditors().Count(), Is.EqualTo(0));
			//Assert.That(Repository.GetSiteSettings(), Is.Not.Null);
			Assert.Fail("TODO");
		}

		[Test]
		public void testconnection_should_succeed_with_valid_connection_string()
		{
			Assert.Fail("TODO");
			//// Arrange


			//// Act
			//Repository.TestConnection(ApplicationSettings.DatabaseName, ApplicationSettings.ConnectionString);

			//// Assert (no exception)
		}

		[Test]
		public void testconnection_should_throw_exception_with_invalid_connection_string()
		{
			Assert.Fail("TODO");
			//// [expectedexception] can't handle exception heirachies

			//// Arrange

			//try
			//{
			//	// Act
			//	// (MongoConnectionException is also thrown here)
			//	Repository.TestConnection(ApplicationSettings.DatabaseName, InvalidConnectionString);
			//}
			//catch (DbException)
			//{
			//	// Assert
			//	Assert.Pass();
			//}
			//catch (ArgumentException)
			//{
			//	Assert.Pass();
			//}
			//catch (Exception)
			//{
			//	Assert.Fail();
			//}
		}
	}
}
