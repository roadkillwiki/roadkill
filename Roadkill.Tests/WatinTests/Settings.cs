using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;

namespace Roadkill.Tests
{
	public class Settings
	{
		public static string ConnectionString { get; private set; }
		public static string BaseUrl { get; private set; }
		public static string AdminUserEmail { get; private set; }
		public static string AdminUserPassword { get; private set; }
		public static string EditorUserEmail { get; private set; }
		public static string EditorUserPassword { get; private set; }

		static Settings()
		{
			ConnectionString = @"server=.\SQLEXPRESS;database=roadkill1.1;integrated security=SSPI";
			BaseUrl = "http://localhost/roadkill.site";

			AdminUserEmail = "admin@localhost";
			AdminUserPassword = "password";

			EditorUserEmail = "editor@localhost";
			EditorUserPassword = "password";

			RoadkillContext.IsWeb = false;
		}
	}
}
