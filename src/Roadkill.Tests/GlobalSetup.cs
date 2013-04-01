using System;
using NUnit.Framework;
using System.IO;
using Roadkill.Core;
using Roadkill.Core.Logging;

// NB no namespace, so this fixture setup is used for every class

[SetUpFixture]
public class GlobalSetup
{
	private static string _rootFolder;
	private static string _libFolder;
	private static string _packagesFolder;

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

	public static string PACKAGES_FOLDER
	{
		get
		{
			if (string.IsNullOrEmpty(_packagesFolder))
			{
				_packagesFolder = Path.Combine(ROOT_FOLDER, "Packages");
			}

			return _packagesFolder;
		}
	}

    /// <summary>
	/// Attempts to copy the correct SQL binaries to the bin folder for the architecture the tests are running under.
	/// </summary>
	[SetUp]
	public void BeforeAllTests()
	{
		Log.UseConsoleLogging();

		//
		// Copy the SQLite interop file
		//
		string binFolder = AppDomain.CurrentDomain.BaseDirectory;
		string sqlInteropFileSource = Path.Combine(PACKAGES_FOLDER, "System.Data.SQLite.1.0.84.0", "content", "net40", "x86", "SQLite.Interop.dll");
		string sqlInteropFileDest = Path.Combine(binFolder, "SQLite.Interop.dll");

		if (!File.Exists(sqlInteropFileDest))
		{
			if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
			{
				sqlInteropFileSource = Path.Combine(PACKAGES_FOLDER, "System.Data.SQLite.1.0.84.0", "content", "net40", "x64", "SQLite.Interop.dll");
			}

			System.IO.File.Copy(sqlInteropFileSource, sqlInteropFileDest, true);
		}
	}
}