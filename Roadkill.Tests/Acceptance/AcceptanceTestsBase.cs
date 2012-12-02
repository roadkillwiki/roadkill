using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SimpleBrowser.WebDriver;

namespace Roadkill.Tests.Acceptance
{
	[Category("Acceptance")]
	public class AcceptanceTestsBase
	{
		protected SimpleBrowserDriver Driver;
		protected string LoginUrl;
		protected string BaseUrl;
		private Process _iisProcess;

		[TestFixtureSetUp]
		public void Setup()
		{
			// Launch IIS Express
			LaunchIisExpress();

			BaseUrl = "http://localhost:9876";
			LoginUrl = BaseUrl + "/user/login";

			Driver = new SimpleBrowserDriver();
			Driver.Navigate().GoToUrl(BaseUrl);
		}

		[TestFixtureTearDown]
		public void TearDown()
		{
			Driver.Dispose();

			if (_iisProcess != null)
			{
				_iisProcess.CloseMainWindow();
				_iisProcess.Dispose();
			}
		}

		private void LaunchIisExpress()
		{
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Roadkill.Site");
			path = new DirectoryInfo(path).FullName; // Don't use "..\..\" format, IIS Express needs a full path

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.Arguments = string.Format("/path:\"{1}\" /port:{2}", "Roadkill Unit Tests", path, 9876);


			string programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			if (Environment.Is64BitOperatingSystem)
				programfiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

			startInfo.FileName = string.Format(@"{0}\IIS Express\iisexpress.exe", programfiles);

			if (!File.Exists(startInfo.FileName))
			{
				throw new FileNotFoundException("IIS Express is not installed and is required for the acceptance tests\n " +
					"Install it from http://www.microsoft.com/en-gb/download/details.aspx?id=1038");
			}

			try
			{
				_iisProcess = Process.Start(startInfo);
			}
			catch
			{
				_iisProcess.CloseMainWindow();
				_iisProcess.Dispose();
			}
		}
	}
}
