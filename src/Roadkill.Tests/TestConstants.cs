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

		public static readonly int WEB_PORT = 9876;
		public static readonly string WEB_PATH;
		public static readonly string WEB_SITENAME = "RoadkillTests";
		public static readonly string WEB_BASEURL = "http://localhost:" +WEB_PORT;

		public static readonly string CONNECTION_STRING;

		static TestConstants()
		{
			// ROOT_FOLDER
			string relativePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..");
			ROOT_FOLDER = new DirectoryInfo(relativePath).FullName;

			// LIB_FOLDER
			LIB_FOLDER = Path.Combine(ROOT_FOLDER, "lib");

			// PACKAGES_FOLDER
			PACKAGES_FOLDER = Path.Combine(ROOT_FOLDER, "Packages");

			// WEB_PATH
			WEB_PATH = Path.Combine(TestConstants.ROOT_FOLDER, "src", "Roadkill.Web");
			WEB_PATH = new DirectoryInfo(WEB_PATH).FullName;

			// CONNECTION_STRING
			// For Appveyor
			string envValue = TestHelpers.GetEnvironmentalVariable("ConnectionString");
			if (!string.IsNullOrEmpty(envValue))
			{
				CONNECTION_STRING = envValue;
			}
			else
			{
				// This should match connectionStrings.dev.config
				CONNECTION_STRING = @"Server=(local);Integrated Security=true;Connect Timeout=5;database=Roadkill";
			}
		}
	}
}