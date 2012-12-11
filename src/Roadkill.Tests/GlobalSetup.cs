using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using NUnit.Framework;
using System.IO;
using Roadkill.Core.Domain;
using Roadkill.Core.Configuration;
using System.Configuration;

// NB no namespace, so this fixture setup is used for every class

[SetUpFixture]
public class GlobalSetup
{
	public static readonly string ROOT_FOLDER;
	public static readonly string LIB_FOLDER;

	static GlobalSetup()
	{
		string relativePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..");

		ROOT_FOLDER = new DirectoryInfo(relativePath).FullName;
		LIB_FOLDER = Path.Combine(ROOT_FOLDER, "lib");

		Console.WriteLine("Using '{0}' for tests ROOT_FOLDER", ROOT_FOLDER);
		Console.WriteLine("Using '{0}' for tests LIB_FOLDER", LIB_FOLDER);
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
		string sqliteFileSource = Path.Combine(binFolder, "lib", "SQLiteBinaries", "x86", "System.Data.SQLite.dll");
		string sqliteFileDest = Path.Combine(binFolder, "System.Data.SQLite.dll");
		string sqliteLinqFileSource = Path.Combine(binFolder, "lib", "SQLiteBinaries", "x86", "System.Data.SQLite.Linq.dll");
		string sqliteFileLinqDest = Path.Combine(binFolder, "System.Data.SQLite.Linq.dll");

		if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
		{
			sqliteFileSource = Path.Combine(binFolder, "lib", "SQLiteBinaries", "x64", "System.Data.SQLite.dll");
			sqliteLinqFileSource = Path.Combine(binFolder, "lib", "SQLiteBinaries", "x64", "System.Data.SQLite.Linq.dll");
		}

		System.IO.File.Copy(sqliteFileSource, sqliteFileDest, true);
		System.IO.File.Copy(sqliteLinqFileSource, sqliteFileLinqDest, true);
	}
}