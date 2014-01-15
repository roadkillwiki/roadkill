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
	/// Used for any future integration/acceptance tests with SQLCE.
	/// </summary>
	public class SqlCeSetup
	{
		public static string ConnectionString { get { return @"Data Source=|DataDirectory|\roadkill-acceptancetests.sdf"; } }

		public static void CopyDb()
		{
			string testsDBPath = Path.Combine(Settings.LIB_FOLDER, "Test-databases", "SqlCe", "roadkill-acceptancetests.sdf");
			File.Copy(testsDBPath, Path.Combine(Settings.WEB_PATH, "App_Data", "roadkill-acceptancetests.sdf"), true);
		}
	}
}
