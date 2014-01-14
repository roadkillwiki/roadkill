using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Tests
{
	public class IISExpress : IDisposable
	{
		private ProcessStartInfo _startInfo;
		public Process IisProcess { get; private set; }

		public IISExpress()
		{
			string sitePath = Settings.WEB_PATH;
			_startInfo = new ProcessStartInfo();
			_startInfo.Arguments = string.Format("/path:\"{0}\" /port:{1}", sitePath, 9876);

			string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			string searchPath1 = string.Format(@"{0}\IIS Express\iisexpress.exe", programFiles);
			string searchPath2 = "";
			_startInfo.FileName = string.Format(@"{0}\IIS Express\iisexpress.exe", programFiles);

			if (!File.Exists(_startInfo.FileName))
			{
				programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
				searchPath2 = string.Format(@"{0}\IIS Express\iisexpress.exe", programFiles);
				_startInfo.FileName = string.Format(@"{0}\IIS Express\iisexpress.exe", programFiles);
			}

			if (!File.Exists(_startInfo.FileName))
			{
				throw new FileNotFoundException(string.Format("IIS Express is not installed in '{0}' or '{1}' and is required for the acceptance/webapi tests\n " +
					"Download it from http://www.microsoft.com/en-gb/download/details.aspx?id=1038",
					searchPath1, searchPath2));
			}
		}

		public void Start()
		{
			try
			{
				Console.WriteLine("Launching IIS Express: {0} {1}", _startInfo.FileName, _startInfo.Arguments);
				IisProcess = Process.Start(_startInfo);
			}
			catch
			{
				IisProcess.CloseMainWindow();
				IisProcess.Dispose();
			}
		}

		public void Dispose()
		{
			if (IisProcess != null && !IisProcess.HasExited)
			{
				IisProcess.CloseMainWindow();
				IisProcess.Dispose();
				Console.WriteLine("Killed IISExpress");
			}
		}
	}
}
