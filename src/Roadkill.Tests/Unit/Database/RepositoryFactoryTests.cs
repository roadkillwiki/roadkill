using System;
using System.Collections.Generic;
using System.Linq;
using Mindscape.LightSpeed;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Schema;
using IRepository = Roadkill.Core.Database.IRepository;

namespace Roadkill.Tests.Unit.Database
{
	public class RepositoryFactoryTests
	{
		[Test]
		public void ListAll_should_return_all_databases()
		{
			// Arrange
			var factory = new RepositoryFactory();

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
		public void GetRepository_should_return_correct_lightspeedrepository(string provider, string connectionString, DataProvider expectedProvider)
		{
			// Arrange
			var factory = new RepositoryFactory();

			// Act
			IRepository repository = factory.GetRepository(provider, connectionString);

			// Assert
			LightSpeedRepository lightSpeedRepository = repository as LightSpeedRepository;
            Assert.That(lightSpeedRepository, Is.Not.Null);
			Assert.That(lightSpeedRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(lightSpeedRepository.DataProvider, Is.EqualTo(expectedProvider));
		}

		[Test]
		public void GetRepository_should_default_to_sqlserver_lightspeedrepository()
		{
			// Arrange
			string provider = "anything";
			string connectionString = "connection-string";
			var factory = new RepositoryFactory();

			// Act
			IRepository repository = factory.GetRepository(provider, connectionString);

			// Assert
			LightSpeedRepository lightSpeedRepository = repository as LightSpeedRepository;
			Assert.That(lightSpeedRepository, Is.Not.Null);
			Assert.That(lightSpeedRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(lightSpeedRepository.DataProvider, Is.EqualTo(DataProvider.SqlServer2008));
		}

		[Test]
		public void GetRepository_should_return_mongodb_repository()
		{
			// Arrange
			string provider = "MONGODB";
			string connectionString = "mongodb-connection-string";
			var factory = new RepositoryFactory();

			// Act
			IRepository repository = factory.GetRepository(provider, connectionString);

			// Assert
			MongoDBRepository mongoDbRepository = repository as MongoDBRepository;
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
			var factory = new RepositoryFactory();

			// Act
			IRepositoryInstaller repository = factory.GetRepositoryInstaller(provider, connectionString);

			// Assert
			LightSpeedRepositoryInstaller lightSpeedRepository = repository as LightSpeedRepositoryInstaller;
			Assert.That(lightSpeedRepository, Is.Not.Null);
			Assert.That(lightSpeedRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(lightSpeedRepository.DataProvider, Is.EqualTo(expectedProvider));
			Assert.That(lightSpeedRepository.Schema, Is.TypeOf(expectedSchemaType));
		}

		[Test]
		public void GetRepositoryInstaller_should_default_to_sqlserver_lightspeedrepository()
		{
			// Arrange
			string provider = "anything";
			string connectionString = "connection-string";
			Type expectedSchemaType = typeof (SqlServerSchema);

            var factory = new RepositoryFactory();

			// Act
			IRepositoryInstaller repository = factory.GetRepositoryInstaller(provider, connectionString);

			// Assert
			LightSpeedRepositoryInstaller lightSpeedRepository = repository as LightSpeedRepositoryInstaller;
			Assert.That(lightSpeedRepository, Is.Not.Null);
			Assert.That(lightSpeedRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(lightSpeedRepository.DataProvider, Is.EqualTo(DataProvider.SqlServer2008));
			Assert.That(lightSpeedRepository.Schema, Is.TypeOf(expectedSchemaType));
		}

		[Test]
		public void GetRepositoryInstaller_should_return_mongodb_repository()
		{
			// Arrange
			string provider = "MONGODB";
			string connectionString = "mongodb-connection-string";
			var factory = new RepositoryFactory();

			// Act
			IRepositoryInstaller repository = factory.GetRepositoryInstaller(provider, connectionString);

			// Assert
			MongoDBRepositoryInstaller mongoDbRepository = repository as MongoDBRepositoryInstaller;
			Assert.That(mongoDbRepository, Is.Not.Null);
			Assert.That(mongoDbRepository.ConnectionString, Is.EqualTo(connectionString));
		}
	}
}
