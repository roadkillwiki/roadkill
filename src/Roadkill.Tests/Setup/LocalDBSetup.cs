using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Tests
{
	public class LocalDBSetup
	{
		public static string ConnectionString { get { return @"Server=(LocalDB)\v11.0;Integrated Security=true;database=master"; } }

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
			string path = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "roadkill-sqlserver-localdb.sql");
			return File.ReadAllText(path);
		}
	}
}
