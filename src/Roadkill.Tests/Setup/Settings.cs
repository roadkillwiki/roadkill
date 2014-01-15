using System;
using NUnit.Framework;
using System.IO;
using Roadkill.Core;
using Roadkill.Core.Logging;
using System.Configuration;

// To turn this into a global setup class, add [SetupFixture] and
// add a [SetUp] method.
// NB make sure there is no namespace, so this fixture setup is used for every class

namespace Roadkill.Tests
{
	public class Settings
	{
		private static string _rootFolder;
		private static string _libFolder;
		private static string _webPath;
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

		public static string WEB_PATH
		{
			get
			{
				if (string.IsNullOrEmpty(_webPath))
				{
					_webPath = Path.Combine(Settings.ROOT_FOLDER, "src", "Roadkill.Web");
					_webPath = new DirectoryInfo(_webPath).FullName;
				}

				return _webPath;
			}
		}

		public static readonly string ADMIN_EMAIL = "admin@localhost";
		public static readonly string ADMIN_PASSWORD = "password";
		public static readonly string EDITOR_EMAIL = "editor@localhost";
		public static readonly string EDITOR_PASSWORD = "password";
		public static readonly Guid ADMIN_ID = new Guid("aabd5468-1c0e-4277-ae10-a0ce00d2fefc");
	}
}