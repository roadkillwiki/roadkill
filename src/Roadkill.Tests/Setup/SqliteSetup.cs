using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Tests.Acceptance;

namespace Roadkill.Tests
{
	/// <summary>
	/// Used for any future integration/acceptance tests with SQLite.
	/// </summary>
	public class SqliteSetup
	{
		public static string ConnectionString { get { return @"Data Source=roadkill-integrationtests.sqlite;"; } }

		public static void CopyInteropFiles()
		{
			// Copy the SQLite interop file for x64
			string binFolder = AppDomain.CurrentDomain.BaseDirectory;
			string sqlInteropFileSource = Path.Combine(Settings.WEB_PATH, "App_Data", "Internal", "SQLiteBinaries", "x86", "SQLite.Interop.dll");
			string sqlInteropFileDest = Path.Combine(binFolder, "SQLite.Interop.dll");

			if (!File.Exists(sqlInteropFileDest))
			{
				//File.Delete(sqlInteropFileDest);

				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
				{
					sqlInteropFileSource = Path.Combine(Settings.WEB_PATH, "App_Data", "Internal", "SQLiteBinaries", "x64", "SQLite.Interop.dll");
				}

				System.IO.File.Copy(sqlInteropFileSource, sqlInteropFileDest, true);
			}
		}

		public static void CopyDb()
		{
			string testsDBPath = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "Sqlite", "roadkill-integrationtests.sqlite");
			File.Copy(testsDBPath, Path.Combine(Settings.WEB_PATH, "App_Data", "roadkill-integrationtests.sqlite"), true);
		}
	}
}
