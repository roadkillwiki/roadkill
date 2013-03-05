using System;
using NUnit.Framework;
using System.IO;

// NB no namespace, so this fixture setup is used for every class

[SetUpFixture]
public class GlobalSetup
{
	private static string _rootFolder;
	private static string _libFolder;

	public static string ROOT_FOLDER
	{
		get
		{
			if (string.IsNullOrEmpty(_rootFolder))
			{
				string relativePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..");

				_rootFolder = new DirectoryInfo(relativePath).FullName;
				Console.WriteLine("Using '{0}' for tests ROOT_FOLDER", ROOT_FOLDER);
			}
			return _rootFolder;
		}
	}

	public static string LIB_FOLDER
	{
		get
		{
			if (string.IsNullOrEmpty(_libFolder))
			{
				_libFolder = Path.Combine(ROOT_FOLDER, "lib");
			}

			return _libFolder;
		}
	}

    /// <summary>
	/// Attempts to copy the correct SQL binaries to the bin folder for the architecture the tests are running under.
	/// </summary>
	[SetUp]
	public void BeforeAllTests()
	{
		//
		// Copy the SQLite files over
		//
		string binFolder = AppDomain.CurrentDomain.BaseDirectory;
		string sqliteFileSource = Path.Combine(LIB_FOLDER, "Test-databases", "SQLite", "x86", "System.Data.SQLite.dll");
		string sqliteLinqFileSource = Path.Combine(LIB_FOLDER, "Test-databases", "SQLite", "x86", "System.Data.SQLite.Linq.dll");
		string sqliteDbFileSource = Path.Combine(LIB_FOLDER, "Test-databases", "roadkill-integrationtests.sqlite");
		
		string sqliteFileDest = Path.Combine(binFolder, "System.Data.SQLite.dll");
		string sqliteFileLinqDest = Path.Combine(binFolder, "System.Data.SQLite.Linq.dll");
		string sqliteDbFileDest = Path.Combine(binFolder, "roadkill-integrationtests.sqlite");

		if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
		{
			sqliteFileSource = Path.Combine(LIB_FOLDER, "SQLite", "x64", "System.Data.SQLite.dll");
			sqliteLinqFileSource = Path.Combine(LIB_FOLDER, "SQLite", "x64", "System.Data.SQLite.Linq.dll");
		}

		System.IO.File.Copy(sqliteFileSource, sqliteFileDest, true);
		System.IO.File.Copy(sqliteLinqFileSource, sqliteFileLinqDest, true);
		System.IO.File.Copy(sqliteDbFileSource, sqliteDbFileDest, true);
	}
}