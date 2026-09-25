using System;
using System.IO;

namespace Roadkill.Tests
{
	public class TestConstants
	{
		public static readonly string ADMIN_EMAIL = "admin@localhost";
		public static readonly string ADMIN_PASSWORD = "password";
		public static readonly string EDITOR_EMAIL = "editor@localhost";
		public static readonly string EDITOR_PASSWORD = "password";
		public static readonly Guid ADMIN_ID = new Guid("aabd5468-1c0e-4277-ae10-a0ce00d2fefc");

		public static readonly string ROOT_FOLDER;
		public static readonly string LIB_FOLDER;
		public static readonly string PACKAGES_FOLDER;

		public static readonly string WEB_PATH;

		/// <summary>
		/// The databases of the integration tests (their tables are dropped and re-created), started in containers on
		/// first use: see <see cref="TestDatabases"/>.
		/// </summary>
		public static string SQLSERVER_CONNECTION_STRING => TestDatabases.SqlServerConnectionString;

		public static string POSTGRES_CONNECTION_STRING => TestDatabases.PostgresConnectionString;

		public static string MONGODB_CONNECTION_STRING => TestDatabases.MongoDBConnectionString;
		
		public static readonly string REST_API_KEY = "apikey1";

		static TestConstants()
		{
			// ROOT_FOLDER - the tests run from src/Roadkill.Tests/bin/{Configuration}/net10.0
			string relativePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..");
			ROOT_FOLDER = new DirectoryInfo(relativePath).FullName;
			LIB_FOLDER = Path.Combine(ROOT_FOLDER, "lib");
			PACKAGES_FOLDER = Path.Combine(ROOT_FOLDER, "packages");
			WEB_PATH = new DirectoryInfo(Path.Combine(ROOT_FOLDER, "src", "Roadkill.Web")).FullName;
		}
	}
}