using System;
using System.IO;
using Microsoft.Data.SqlClient;
using Npgsql;

namespace Roadkill.Tests
{
	public class TestHelpers
	{
		/// <summary>
		/// MongoDB isn't tested: set the ROADKILL_MONGODB_TESTS environment variable to "true" to run its integration tests.
		/// </summary>
		public static bool IsMongoDBRunning()
		{
			return string.Equals(Environment.GetEnvironmentVariable("ROADKILL_MONGODB_TESTS"), "true", StringComparison.OrdinalIgnoreCase);
		}

		public static string GetEnvironmentalVariable(string name)
		{
			return Environment.GetEnvironmentVariable(name);
		}

		public class SqlServerSetup
		{
			public static void RecreateTables()
			{
				using (SqlConnection connection = new SqlConnection(TestConstants.SQLSERVER_CONNECTION_STRING))
				{
					connection.Open();

					SqlCommand command = connection.CreateCommand();
					command.CommandText = ReadSqlServerScript();

					command.ExecuteNonQuery();
				}
			}

			public static void ClearDatabase()
			{
				using (SqlConnection connection = new SqlConnection(TestConstants.SQLSERVER_CONNECTION_STRING))
				{
					connection.Open();

					SqlCommand command = connection.CreateCommand();
					command.CommandText =
						"DELETE FROM roadkill_pagecontent;" +
						"DELETE FROM roadkill_pages;" +
						"DELETE FROM roadkill_users;" +
						"DELETE FROM roadkill_siteconfiguration;" +
						"DBCC CHECKIDENT (roadkill_pages, RESEED, 1);";

					command.ExecuteNonQuery();
				}
			}

			private static string ReadSqlServerScript()
			{
				string path = Path.Combine(TestConstants.LIB_FOLDER, "Test-databases", "roadkill-sqlserver.sql");
				return File.ReadAllText(path);
			}
		}

		//
		// Setup instructions
		// docker run --name some-postgres -p 5432:5432 -e POSTGRES_PASSWORD=mysecretpassword -d postgres
		// Download EMS SQL Manager for PostgreSQL Freeware:
		//   - http://www.sqlmanager.net/en/products/postgresql/manager/download
		// Create a database called "roadkill"
		//
		public class PostgresSetup
		{
			public static void RecreateTables()
			{
				using (NpgsqlConnection connection = new NpgsqlConnection(TestConstants.POSTGRES_CONNECTION_STRING))
				{
					connection.Open();

					NpgsqlCommand command = connection.CreateCommand();
					command.CommandText = ReadSqlServerScript();

					command.ExecuteNonQuery();
				}
			}

			public static void ClearDatabase()
			{
				using (NpgsqlConnection connection = new NpgsqlConnection(TestConstants.POSTGRES_CONNECTION_STRING))
				{
					connection.Open();

					NpgsqlCommand command = connection.CreateCommand();
					command.CommandText =
						"DELETE FROM roadkill_pagecontent;" +
						"DELETE FROM roadkill_pages;" +
						"DELETE FROM roadkill_users;" +
						"DELETE FROM roadkill_siteconfiguration;";

					command.ExecuteNonQuery();
				}
			}

			private static string ReadSqlServerScript()
			{
				string path = Path.Combine(TestConstants.LIB_FOLDER, "Test-databases", "roadkill-postgres.sql");
				return File.ReadAllText(path);
			}
		}
	}
}
