using System;
using System.Collections.Generic;
using System.Linq;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Tests.Unit.Database
{
	public class RepositoryFactoryTests
	{
		private void SetUnitOfWork(RepositoryFactory factory)
		{
			factory.UnitOfWorkFunc = context => new UnitOfWork();
		}

		[Test]
		public void listall_should_return_all_databases()
		{
			// Arrange
			var factory = new RepositoryFactory("database name", "not empty");
			SetUnitOfWork(factory);

			// Act
			List<RepositoryInfo> all = factory.ListAll().ToList();

			// Assert
			Assert.That(all.Count, Is.EqualTo(4));
			Assert.That(all.First(), Is.Not.Null);
			Assert.That(all.First().Id, Is.Not.Null.Or.Empty);
		}

		[Test]
		[TestCase("PostGres", "my-postgres-connection-string", DataProvider.PostgreSql9)]
		[TestCase("Mysql", "myql-connection-string", DataProvider.MySql5)]
		[TestCase("sqlserver", "my-sqlserver-connection-string", DataProvider.SqlServer2008)]
		[TestCase("anything", "connection-string", DataProvider.SqlServer2008)]
		public void GetSettingsRepository_should_return_correct_lightspeedrepository(string provider, string connectionString, DataProvider expectedProvider)
		{
			// Arrange
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			ISettingsRepository repository = factory.GetSettingsRepository(provider, connectionString);

			// Assert
			LightSpeedSettingsRepository lightSpeedRepository = repository as LightSpeedSettingsRepository;
			Assert.That(lightSpeedRepository, Is.Not.Null);
		}

		[Test]
		public void GetSettingsRepository_should_default_to_sqlserver_lightspeedrepository()
		{
			// Arrange
			string provider = "anything";
			string connectionString = "connection-string";
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			ISettingsRepository repository = factory.GetSettingsRepository(provider, connectionString);

			// Assert
			LightSpeedSettingsRepository lightSpeedRepository = repository as LightSpeedSettingsRepository;
			Assert.That(lightSpeedRepository, Is.Not.Null);
		}

		[Test]
		public void GetSettingsRepository_should_return_mongodb_repository()
		{
			// Arrange
			string provider = "MONGODB";
			string connectionString = "mongodb-connection-string";
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			ISettingsRepository repository = factory.GetSettingsRepository(provider, connectionString);

			// Assert
			MongoDBSettingsRepository mongoDbRepository = repository as MongoDBSettingsRepository;
			Assert.That(mongoDbRepository, Is.Not.Null);
			Assert.That(mongoDbRepository.ConnectionString, Is.EqualTo(connectionString));
		}

		[Test]
		[TestCase("PostGres", "my-postgres-connection-string", DataProvider.PostgreSql9)]
		[TestCase("Mysql", "myql-connection-string", DataProvider.MySql5)]
		[TestCase("sqlserver", "my-sqlserver-connection-string", DataProvider.SqlServer2008)]
		[TestCase("anything", "connection-string", DataProvider.SqlServer2008)]
		public void GetUserRepository_should_return_correct_lightspeedrepository(string provider, string connectionString, DataProvider expectedProvider)
		{
			// Arrange
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IUserRepository repository = factory.GetUserRepository(provider, connectionString);

			// Assert
			LightSpeedUserRepository lightSpeedRepository = repository as LightSpeedUserRepository;
			Assert.That(lightSpeedRepository, Is.Not.Null);
		}

		[Test]
		public void GetUserRepository_should_default_to_sqlserver_lightspeedrepository()
		{
			// Arrange
			string provider = "anything";
			string connectionString = "connection-string";
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IUserRepository repository = factory.GetUserRepository(provider, connectionString);

			// Assert
			LightSpeedUserRepository lightSpeedRepository = repository as LightSpeedUserRepository;
			Assert.That(lightSpeedRepository, Is.Not.Null);
		}

		[Test]
		public void GetUserRepository_should_return_mongodb_repository()
		{
			// Arrange
			string provider = "MONGODB";
			string connectionString = "mongodb-connection-string";
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IUserRepository repository = factory.GetUserRepository(provider, connectionString);

			// Assert
			MongoDBUserRepository mongoDbRepository = repository as MongoDBUserRepository;
			Assert.That(mongoDbRepository, Is.Not.Null);
			Assert.That(mongoDbRepository.ConnectionString, Is.EqualTo(connectionString));
		}

		[Test]
		[TestCase("PostGres", "my-postgres-connection-string", DataProvider.PostgreSql9)]
		[TestCase("Mysql", "myql-connection-string", DataProvider.MySql5)]
		[TestCase("sqlserver", "my-sqlserver-connection-string", DataProvider.SqlServer2008)]
		[TestCase("anything", "connection-string", DataProvider.SqlServer2008)]
		public void GetPageRepository_should_return_correct_lightspeedrepository(string provider, string connectionString, DataProvider expectedProvider)
		{
			// Arrange
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IPageRepository repository = factory.GetPageRepository(provider, connectionString);

			// Assert
			LightSpeedPageRepository lightSpeedRepository = repository as LightSpeedPageRepository;
            Assert.That(lightSpeedRepository, Is.Not.Null);
		}

		[Test]
		public void GetPageRepository_should_default_to_sqlserver_lightspeedrepository()
		{
			// Arrange
			string provider = "anything";
			string connectionString = "connection-string";
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IPageRepository repository = factory.GetPageRepository(provider, connectionString);

			// Assert
			LightSpeedPageRepository lightSpeedRepository = repository as LightSpeedPageRepository;
			Assert.That(lightSpeedRepository, Is.Not.Null);
		}

		[Test]
		public void GetPageRepository_should_return_mongodb_repository()
		{
			// Arrange
			string provider = "MONGODB";
			string connectionString = "mongodb-connection-string";
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IPageRepository repository = factory.GetPageRepository(provider, connectionString);

			// Assert
			MongoDBPageRepository mongoDbRepository = repository as MongoDBPageRepository;
			Assert.That(mongoDbRepository, Is.Not.Null);
			Assert.That(mongoDbRepository.ConnectionString, Is.EqualTo(connectionString));
		}

		[Test]
		[TestCase("PostGres", "my-postgres-connection-string", DataProvider.PostgreSql9, typeof(PostgresSchema))]
		[TestCase("Mysql", "myql-connection-string", DataProvider.MySql5, typeof(MySqlSchema))]
		[TestCase("sqlserver", "my-sqlserver-connection-string", DataProvider.SqlServer2008, typeof(SqlServerSchema))]
		[TestCase("anything", "connection-string", DataProvider.SqlServer2008, typeof(SqlServerSchema))]
		public void GetRepositoryInstaller_should_return_correct_lightspeedrepository(string provider, string connectionString, DataProvider expectedProvider, Type expectedSchemaType)
		{
			// Arrange
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IInstallerRepository installerRepository = factory.GetInstallerRepository(provider, connectionString);

			// Assert
			LightSpeedInstallerRepository lightSpeedInstallerRepository = installerRepository as LightSpeedInstallerRepository;
			Assert.That(lightSpeedInstallerRepository, Is.Not.Null);
			Assert.That(lightSpeedInstallerRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(lightSpeedInstallerRepository.DataProvider, Is.EqualTo(expectedProvider));
			Assert.That(lightSpeedInstallerRepository.Schema, Is.TypeOf(expectedSchemaType));
		}

		[Test]
		public void getrepositoryinstaller_should_default_to_sqlserver_lightspeedrepository()
		{
			// Arrange
			string provider = "anything";
			string connectionString = "connection-string";
			Type expectedSchemaType = typeof (SqlServerSchema);

            var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IInstallerRepository installerRepository = factory.GetInstallerRepository(provider, connectionString);

			// Assert
			LightSpeedInstallerRepository lightSpeedInstallerRepository = installerRepository as LightSpeedInstallerRepository;
			Assert.That(lightSpeedInstallerRepository, Is.Not.Null);
			Assert.That(lightSpeedInstallerRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(lightSpeedInstallerRepository.DataProvider, Is.EqualTo(DataProvider.SqlServer2008));
			Assert.That(lightSpeedInstallerRepository.Schema, Is.TypeOf(expectedSchemaType));
		}

		[Test]
		public void getrepositoryinstaller_should_return_mongodb_repository()
		{
			// Arrange
			string provider = "MONGODB";
			string connectionString = "mongodb-connection-string";
			var factory = new RepositoryFactory(provider, connectionString);
			SetUnitOfWork(factory);

			// Act
			IInstallerRepository installerRepository = factory.GetInstallerRepository(provider, connectionString);

			// Assert
			MongoDbInstallerRepository mongoDbInstallerRepository = installerRepository as MongoDbInstallerRepository;
			Assert.That(mongoDbInstallerRepository, Is.Not.Null);
			Assert.That(mongoDbInstallerRepository.ConnectionString, Is.EqualTo(connectionString));
		}
	}
}
