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

public class GlobalSetup
{
	/// <summary>
	/// Attempts to copy the correct SQL binaries to the bin folder for the architecture the app pool is running under.
	/// </summary>
	[TestFixtureSetUp]
	public void Init()
	{
		RoadkillApplication.SetupIoC();

		//
		// Copy the SQLite files over
		//
		string rootFolder = AppDomain.CurrentDomain.BaseDirectory;
		string sqliteFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x86", "System.Data.SQLite.dll");
		string sqliteFileDest = Path.Combine(rootFolder, "System.Data.SQLite.dll");
		string sqliteLinqFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x86", "System.Data.SQLite.Linq.dll");
		string sqliteFileLinqDest = Path.Combine(rootFolder, "System.Data.SQLite.Linq.dll");

		if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
		{
			sqliteFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x64", "System.Data.SQLite.dll");
			sqliteLinqFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x64", "System.Data.SQLite.Linq.dll");
		}

		System.IO.File.Copy(sqliteFileSource, sqliteFileDest, true);
		System.IO.File.Copy(sqliteLinqFileSource, sqliteFileLinqDest, true);
	}
}
