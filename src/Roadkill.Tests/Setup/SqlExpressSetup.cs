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
	public class SqlExpressSetup
	{
		public static string ConnectionString
		{
			get
			{
				string envValue = EnvironmentalVariables.GetVariable("ConnectionString");
				if (!string.IsNullOrEmpty(envValue))
				{
					Console.WriteLine("Found {0} connection string environmental variable.");
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

		private static string ReadSqlServerScript()
		{
			string path = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "roadkill-sqlserver.sql");
			return File.ReadAllText(path);
		}
	}
}
