using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Plasma.Core;
using NUnit.Framework;
using System.Text.RegularExpressions;

namespace Roadkill.Tests.Plasma
{
	/// <summary>
	/// Plasma hosts the site inside the same app domain as the tests, skipping IIS.
	/// These tests are alternatives to Selenium UI tests - they only scrape the HTML, they
	/// can't test Javascript functionality.
	/// 
	/// For Plasma to work on your desktop machine, compile using the Plasma configuration.
	/// </summary>
	[TestFixture]
	public class PlasmaTestBase
	{
		protected static AspNetApplication AppInstance;

		[TestFixtureSetUp]
		public void Setup()
		{
			try
			{
				string siteRootFolder = AppDomain.CurrentDomain.BaseDirectory;

				// Atalassian AMI uses C:\build-dir\REPONAME\_PLASMAWEBSITE
				// Tests are run inside C:\build-dir\REPONAME\Roadkill.Tests\bin\release
				// Copy the SQLite into the _PLASMAWEBSITE folder.

				if (Environment.UserInteractive)
				{
					// desktop builds
					siteRootFolder = Path.Combine(siteRootFolder, "..", "..", "..", "Roadkill.Site");	
				}
				else
				{
					siteRootFolder = Path.Combine(siteRootFolder, "..", "..", "..", "_PLASMAWEBSITE");
				}

				CopySqliteDatabaseToSite(siteRootFolder);
				CopySqliteToSite(siteRootFolder);

				DirectoryInfo siteDirectory = new DirectoryInfo(siteRootFolder);
				AppInstance = new AspNetApplication("/", siteDirectory.FullName);
			}
			catch (Exception e)
			{
				Assert.Fail(e.StackTrace.ToString());
			}
		}

		private void CopySqliteDatabaseToSite(string siteRootFolder)
		{
			string testBinFolder = AppDomain.CurrentDomain.BaseDirectory;

			string source = Path.Combine(testBinFolder, "lib", "roadkill.plasma.sqlite");
			string dest = Path.Combine(siteRootFolder, "App_Data", "roadkill.plasma.sqlite");

			FileInfo destInfo = new FileInfo(dest);
			if (!destInfo.Exists)
				destInfo.Directory.Create();

			System.IO.File.Copy(source, dest, true);
		}

		private void CopySqliteToSite(string siteRootFolder)
		{
			try
			{
				//
				// Copy the sqlite database into the App_Data folder
				// Copy the sqlite binaries from app_data into bin for the site root.
				//
				string testRootFolder = AppDomain.CurrentDomain.BaseDirectory;
				string siteAppData = Path.Combine(siteRootFolder, "App_Data");
				string siteBinFolder = Path.Combine(siteRootFolder, "bin");

				string sqliteFileSource = Path.Combine(siteAppData, "SQLiteBinaries", "x86", "System.Data.SQLite.dll");
				string sqliteFileDest = Path.Combine(siteBinFolder, "System.Data.SQLite.dll");
				string sqliteLinqFileSource = Path.Combine(siteAppData, "SQLiteBinaries", "x86", "System.Data.SQLite.Linq.dll");
				string sqliteFileLinqDest = Path.Combine(siteBinFolder, "System.Data.SQLite.Linq.dll");

				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
				{
					sqliteFileSource = Path.Combine(siteAppData, "SQLiteBinaries", "x64", "System.Data.SQLite.dll");
					sqliteLinqFileSource = Path.Combine(siteAppData, "SQLiteBinaries", "x64", "System.Data.SQLite.Linq.dll");
				}

				FileInfo destInfo = new FileInfo(sqliteFileDest);
				if (!destInfo.Exists)
					destInfo.Directory.Create();

				destInfo = new FileInfo(sqliteFileLinqDest);
				if (!destInfo.Exists)
					destInfo.Directory.Create();
				
				System.IO.File.Copy(sqliteFileSource, sqliteFileDest, true);
				System.IO.File.Copy(sqliteLinqFileSource, sqliteFileLinqDest, true);
			}
			catch (Exception e)
			{
				// Could be ignored
				throw e;
			}
		}

		[TestFixtureTearDown]
		public static void Cleanup()
		{
			AppInstance.Close();
		}

		protected string ParseFollowUrl(string html)
		{
			//<html><head><title>Object moved</title></head><body>
			//<h2>Object moved to <a href="/?nocache=634797034179618895">here</a>.</h2>
			//</body></html>

			Regex regex = new Regex("Object moved to <a href=\"(.*?)\"");
			return regex.Match(html).Groups[1].Value;
		}
	}
}
