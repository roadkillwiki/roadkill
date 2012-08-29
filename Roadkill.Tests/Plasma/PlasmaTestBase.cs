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
		public void Init()
		{
			try
			{
				string siteRootFolder = AppDomain.CurrentDomain.BaseDirectory;

				// Atalassian AMI uses C:\build-dir\REPONAME\_PUBLISHEDWEBSITE
				// Tests are run inside C:\build-dir\REPONAME\Roadkill.Tests\bin\release
				// Copy the SQLite into the publishedwebsite folder.
				siteRootFolder = Path.Combine(siteRootFolder, "..", "..", "..", "_PublishedWebsite");
				CopySqliteToSite(siteRootFolder);

				DirectoryInfo siteDirectory = new DirectoryInfo(siteRootFolder);
				AppInstance = new AspNetApplication("/", siteDirectory.FullName);
			}
			catch (Exception e)
			{
				Assert.Fail(e.StackTrace.ToString());
			}
		}

		private void CopySqliteToSite(string rootFolder)
		{
			try
			{
				//
				// Copy the sqlite database into the App_Data folder
				// Copy the sqlite binaries from app_data into bin for the site root.
				//
				string testRootFolder = AppDomain.CurrentDomain.BaseDirectory;
				string appData = Path.Combine(rootFolder, "App_Data");
				string binFolder = Path.Combine(rootFolder, "bin");

				string sqliteFileSource = Path.Combine(appData, "SQLiteBinaries", "x86", "System.Data.SQLite.dll");
				string sqliteFileDest = Path.Combine(binFolder, "System.Data.SQLite.dll");
				string sqliteLinqFileSource = Path.Combine(appData, "SQLiteBinaries", "x86", "System.Data.SQLite.Linq.dll");
				string sqliteFileLinqDest = Path.Combine(binFolder, "System.Data.SQLite.Linq.dll");

				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
				{
					sqliteFileSource = Path.Combine(appData, "SQLiteBinaries", "x64", "System.Data.SQLite.dll");
					sqliteLinqFileSource = Path.Combine(appData, "SQLiteBinaries", "x64", "System.Data.SQLite.Linq.dll");
				}

				
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
