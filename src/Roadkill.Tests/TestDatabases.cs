using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using NUnit.Framework;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace Roadkill.Tests
{
	/// <summary>
	/// Starts the databases of the integration tests in throwaway containers (Testcontainers, with Docker or a Podman
	/// machine), on first use. The databases to test and the container engine are set in the test project's appsettings.json.
	/// </summary>
	public static class TestDatabases
	{
		public const string SqlServer = "SqlServer";
		public const string Postgres = "Postgres";
		public const string MongoDB = "MongoDB";

		private const string DatabaseName = "roadkilltests";

		private static readonly Lazy<Settings> _settings = new Lazy<Settings>(LoadSettings);
		private static readonly Lazy<string> _sqlServer = new Lazy<string>(() => Start(SqlServer, StartSqlServer));
		private static readonly Lazy<string> _postgres = new Lazy<string>(() => Start(Postgres, StartPostgres));
		private static readonly Lazy<string> _mongoDB = new Lazy<string>(() => Start(MongoDB, StartMongoDB));
		private static readonly List<IContainer> _containers = new List<IContainer>();

		/// <summary>
		/// The SQL Server connection string (the test is ignored if SQL Server isn't in the databases to test).
		/// </summary>
		public static string SqlServerConnectionString => GetConnectionString(SqlServer, _sqlServer);

		public static string PostgresConnectionString => GetConnectionString(Postgres, _postgres);

		public static string MongoDBConnectionString => GetConnectionString(MongoDB, _mongoDB);

		private static string GetConnectionString(string database, Lazy<string> connectionString)
		{
			if (!_settings.Value.Databases.Contains(database, StringComparer.OrdinalIgnoreCase))
				Assert.Ignore($"{database} isn't in IntegrationTests:Databases (appsettings.json of Roadkill.Tests)");

			// If the container couldn't be started, the exception is cached by the Lazy: all the tests of this database fail
			return connectionString.Value;
		}

		private static string Start(string database, Func<string> start)
		{
			TestcontainersSettings.ResourceReaperEnabled = _settings.Value.ResourceReaper;

			try
			{
				return start();
			}
			catch (Exception ex)
			{
				throw new InvalidOperationException($"The {database} container couldn't be started. Start Docker or the Podman machine " +
					"(podman machine start), or set IntegrationTests:ContainerEngineEndpoint in the appsettings.json of Roadkill.Tests. " +
					$"({ex.GetType().Name}: {ex.Message})", ex);
			}
		}

		private static string StartSqlServer()
		{
			// Generated for each run: the container is thrown away at the end of the tests
			string password = "Rk1!" + Guid.NewGuid().ToString("N");

			MsSqlBuilder builder = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest").WithPassword(password);
			if (HasEndpoint)
				builder = builder.WithDockerEndpoint(_settings.Value.ContainerEngineEndpoint);

			MsSqlContainer container = builder.Build();
			StartContainer(container);

			string server = $"{container.Hostname},{container.GetMappedPublicPort(MsSqlBuilder.MsSqlPort)}";
			using (var connection = new SqlConnection($"Server={server};Database=master;User ID=sa;Password={password};TrustServerCertificate=true"))
			{
				connection.Open();
				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandText = $"CREATE DATABASE {DatabaseName}";
					command.ExecuteNonQuery();
				}
			}

			return $"Server={server};Database={DatabaseName};User ID=sa;Password={password};TrustServerCertificate=true;Connect Timeout=5";
		}

		private static string StartPostgres()
		{
			string password = Guid.NewGuid().ToString("N");

			PostgreSqlBuilder builder = new PostgreSqlBuilder("postgres:16")
				.WithDatabase(DatabaseName)
				.WithUsername("postgres")
				.WithPassword(password);
			if (HasEndpoint)
				builder = builder.WithDockerEndpoint(_settings.Value.ContainerEngineEndpoint);

			PostgreSqlContainer container = builder.Build();
			StartContainer(container);

			return $"User ID=postgres;Password={password};Host={container.Hostname};Port={container.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort)};Database={DatabaseName};";
		}

		private static string StartMongoDB()
		{
			// No authentication: the container is only reachable from this machine and thrown away at the end of the tests
			MongoDbBuilder builder = new MongoDbBuilder("mongo:8.0").WithUsername("").WithPassword("");
			if (HasEndpoint)
				builder = builder.WithDockerEndpoint(_settings.Value.ContainerEngineEndpoint);

			MongoDbContainer container = builder.Build();
			StartContainer(container);

			// The Roadkill MongoDB repositories use the database name of the connection string
			var url = new MongoUrlBuilder(container.GetConnectionString()) { DatabaseName = DatabaseName };
			return url.ToString();
		}

		private static bool HasEndpoint => !string.IsNullOrWhiteSpace(_settings.Value.ContainerEngineEndpoint);

		private static void StartContainer(IContainer container)
		{
			lock (_containers)
			{
				_containers.Add(container);
			}

			container.StartAsync().GetAwaiter().GetResult();
		}

		/// <summary>
		/// Removes the containers started by the test run.
		/// </summary>
		internal static void StopAll()
		{
			lock (_containers)
			{
				foreach (IContainer container in _containers)
				{
					container.DisposeAsync().AsTask().GetAwaiter().GetResult();
				}

				_containers.Clear();
			}
		}

		private static Settings LoadSettings()
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false)
				.Build();

			var settings = new Settings();
			configuration.GetSection("IntegrationTests").Bind(settings);
			return settings;
		}

		private class Settings
		{
			public string[] Databases { get; set; } = Array.Empty<string>();
			public string ContainerEngineEndpoint { get; set; }
			public bool ResourceReaper { get; set; } = true;
		}
	}

	/// <summary>
	/// Removes the database containers at the end of the test run (it's in the root namespace, so it applies to all the tests).
	/// </summary>
	[SetUpFixture]
	public class TestDatabasesCleanup
	{
		[OneTimeTearDown]
		public void StopContainers()
		{
			TestDatabases.StopAll();
		}
	}
}
