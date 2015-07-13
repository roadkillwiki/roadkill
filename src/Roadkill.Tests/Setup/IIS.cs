using System;
using System.IO;
using IisConfiguration;
using IisConfiguration.Logging;
using Microsoft.Web.Administration;

namespace Roadkill.Tests
{
	public class IIS : IDisposable
	{
		private const string _siteName = "RoadkillTests";
		private const int _webPort = 9876;

		public void Start()
		{
			var logger = new ConsoleLogger();
			var serverConfig = new WebServerConfig(logger);

			// Current directory: src\Roadkill.Tests\bin\Debug
			string webRoot = Environment.CurrentDirectory + @"..\..\..\..\Roadkill.Web";
			var dirInfo = new DirectoryInfo(webRoot);

			serverConfig
				.AddAppPool(_siteName, "v4.0", ManagedPipelineMode.Integrated, ProcessModelIdentityType.LocalService)
				.WithProcessModel(TimeSpan.FromMinutes(60), false)
				.Commit();

			serverConfig
				.AddSite(_siteName, _webPort, _webPort)
				.AddApplication("/", dirInfo.FullName, _siteName)
				.WithLogging(false)
				.Commit();
		}

		public void Dispose()
		{
			
		}
	}
}