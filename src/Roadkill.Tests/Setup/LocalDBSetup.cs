using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LocalDbApi;

namespace Roadkill.Tests
{
	public class LocalDBSetup
	{
		public static string ConnectionString { get { return @"Server=(LocalDB)\Roadkill;Integrated Security=true;database=TempDB"; } }
		public Instance LocalDbInstance;

		public void StartLocalDB()
		{
			LocalDbInstance = new Instance();
			LocalDbInstance.Create("Roadkill");
		}

		public void StopLocalDB()
		{
			if (LocalDbInstance != null)
			{
				LocalDbInstance.Delete("Roadkill");
				LocalDbInstance.StopInstance("Roadkill");
			}
		}

		public void RecreateLocalDbData()
		{
			using (SqlConnection connection = new SqlConnection(ConnectionString))
			{
				connection.Open();

				SqlCommand command = connection.CreateCommand();
				command.CommandText = ReadSqlServerScript();

				command.ExecuteNonQuery();
			}
		}

		private string ReadSqlServerScript()
		{
			string path = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "roadkill-sqlserver-localdb.sql");
			return File.ReadAllText(path);
		}
	}
}
