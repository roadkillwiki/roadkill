using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Database.Repositories.Dapper;
using Roadkill.Core.Database.Schema;

namespace Roadkill.Tests.Unit.Database
{
	[TestFixture]
	[Category("Unit")]
	public class RepositoryFactoryTests
	{
		[Test]
		public void listall_should_return_all_databases()
		{
			// Arrange
			var factory = new RepositoryFactory();

			// Act
			List<RepositoryInfo> all = factory.ListAll().ToList();

			// Assert
			Assert.That(all.Count, Is.EqualTo(3));
			Assert.That(all.Select(x => x.Id), Is.EquivalentTo(new[] { "MongoDB", "Postgres", "SqlServer2008" }));
		}

		[Test]
		[TestCase("PostGres", typeof(PostgresConnectionFactory))]
		[TestCase("sqlserver2008", typeof(SqlConnectionFactory))]
		[TestCase("SqlServer2012", typeof(SqlConnectionFactory))]
		[TestCase("anything", typeof(SqlConnectionFactory))]
		[TestCase("", typeof(SqlConnectionFactory))]
		public void CreateConnectionFactory_should_return_correct_factory_and_default_to_sqlserver(string provider, Type expectedType)
		{
			// Arrange + Act
			IDbConnectionFactory connectionFactory = RepositoryFactory.CreateConnectionFactory(provider, "connection-string");

			// Assert
			Assert.That(connectionFactory, Is.TypeOf(expectedType));
		}

		[Test]
		[TestCase("Postgres")]
		[TestCase("SqlServer2008")]
		public void repositories_should_be_dapper_repositories_for_sql_databases(string provider)
		{
			// Arrange
			var factory = new RepositoryFactory();

			// Act + Assert
			Assert.That(factory.GetSettingsRepository(provider, "connection-string"), Is.TypeOf<DapperSettingsRepository>());
			Assert.That(factory.GetUserRepository(provider, "connection-string"), Is.TypeOf<DapperUserRepository>());
			Assert.That(factory.GetPageRepository(provider, "connection-string"), Is.TypeOf<DapperPageRepository>());
			Assert.That(factory.GetInstallerRepository(provider, "connection-string"), Is.TypeOf<DapperInstallerRepository>());
		}

		[Test]
		[TestCase("Postgres", typeof(PostgresSchema))]
		[TestCase("SqlServer2008", typeof(SqlServerSchema))]
		public void GetInstallerRepository_should_use_schema_for_database(string provider, Type expectedSchema)
		{
			// Arrange
			var factory = new RepositoryFactory();

			// Act
			var repository = factory.GetInstallerRepository(provider, "connection-string") as DapperInstallerRepository;

			// Assert
			Assert.That(repository, Is.Not.Null);
			Assert.That(repository.Schema, Is.TypeOf(expectedSchema));
		}

		[Test]
		public void repositories_should_be_mongodb_repositories_for_mongodb()
		{
			// Arrange
			var factory = new RepositoryFactory();
			string connectionString = "mongodb://localhost/roadkill";

			// Act
			var settingsRepository = factory.GetSettingsRepository("MONGODB", connectionString) as MongoDBSettingsRepository;

			// Assert
			Assert.That(settingsRepository, Is.Not.Null);
			Assert.That(settingsRepository.ConnectionString, Is.EqualTo(connectionString));
			Assert.That(factory.GetUserRepository("MongoDB", connectionString), Is.TypeOf<MongoDBUserRepository>());
			Assert.That(factory.GetPageRepository("MongoDB", connectionString), Is.TypeOf<MongoDBPageRepository>());
			Assert.That(factory.GetInstallerRepository("MongoDB", connectionString), Is.TypeOf<MongoDbInstallerRepository>());
		}

		[Test]
		public void repositories_should_be_null_when_the_connection_string_is_empty()
		{
			// Arrange (Roadkill isn't installed)
			var factory = new RepositoryFactory();

			// Act + Assert
			Assert.That(factory.GetSettingsRepository("SqlServer2008", ""), Is.Null);
			Assert.That(factory.GetUserRepository("SqlServer2008", ""), Is.Null);
			Assert.That(factory.GetPageRepository("SqlServer2008", ""), Is.Null);
		}
	}
}
