using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Tests.Setup;

namespace Roadkill.Tests
{
	public class SqlServerSetup
	{
		public static string ConnectionString
		{
			get
			{
				// For Appveyor
				string envValue = EnvironmentalVariables.GetVariable("ConnectionString");
				if (!string.IsNullOrEmpty(envValue))
				{
					return envValue;
				}

				// This should match connectionStrings.dev.config
				return @"Server=(local);Integrated Security=true;Connect Timeout=5;database=Roadkill";
			}
		}

		public static void RecreateLocalDbData()
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();

				SqlCommand command = connection.CreateCommand();
				command.CommandText = ReadSqlServerScript();

				command.ExecuteNonQuery();
			}
		}

		public static void ClearDatabase()
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();

				SqlCommand command = connection.CreateCommand();
				command.CommandText =
					"DELETE FROM roadkill_pagecontent;"+
					"DELETE FROM roadkill_pages;"+
					"DELETE FROM roadkill_users;"+
					"DELETE FROM roadkill_siteconfiguration;"+
					"DBCC CHECKIDENT (roadkill_pages, RESEED, 1);";

				command.ExecuteNonQuery();
			}
		}

		private static string ReadSqlServerScript()
		{
			string path = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "roadkill-sqlserver.sql");
			return File.ReadAllText(path);
		}
	}
}
