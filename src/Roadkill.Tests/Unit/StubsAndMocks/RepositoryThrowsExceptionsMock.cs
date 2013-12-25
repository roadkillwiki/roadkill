using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;
using PluginSettings = Roadkill.Core.Plugins.Settings;

namespace Roadkill.Tests.Unit
{
	public class RepositoryThrowsExceptionsMock : RepositoryMock
	{
		public static string ExceptionMessage = "A big mocking oops";

		public override void Install(DataStoreType dataStoreType, string connectionString, bool enableCache)
		{
			throw new Exception(ExceptionMessage);
		}

		public override void TestConnection(DataStoreType dataStoreType, string connectionString)
		{
			throw new Exception(ExceptionMessage);
		}
	}
}
