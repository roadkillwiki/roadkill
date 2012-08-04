using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Roadkill.Core;
using Roadkill.Tests.Core;
using NUnit.Framework;
using System.IO;

using Plasma.Core;
using Plasma.WebDriver;
using OpenQA.Selenium;
using System.Diagnostics;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace Roadkill.Tests.Plasma
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestFixture]
	public class PageTests
	{
		private static AspNetApplication _appInstance;

		static PageTests()
		{
			CopySqliteToSite();

			// Set the Plasma root directory to the site's directory
			string rootFolder = AppDomain.CurrentDomain.BaseDirectory;

			rootFolder = Path.Combine(rootFolder, "..", "..", "..", "Roadkill.Site");
			DirectoryInfo siteDirectory = new DirectoryInfo(rootFolder);
			_appInstance = new AspNetApplication("/", siteDirectory.FullName); 
		}

		private static void CopySqliteToSite()
		{
			try
			{
				//
				// Copy the SQLite files over
				//
				string rootFolder = AppDomain.CurrentDomain.BaseDirectory;
				string destination = AppDomain.CurrentDomain.BaseDirectory;
				rootFolder = Path.Combine(rootFolder, "..", "..", "..", "Roadkill.Site", "bin");

				string sqliteFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x86", "System.Data.SQLite.dll");
				string sqliteFileDest = Path.Combine(destination, "System.Data.SQLite.dll");
				string sqliteLinqFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x86", "System.Data.SQLite.Linq.dll");
				string sqliteFileLinqDest = Path.Combine(destination, "System.Data.SQLite.Linq.dll");

				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
				{
					sqliteFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x64", "System.Data.SQLite.dll");
					sqliteLinqFileSource = Path.Combine(rootFolder, "lib", "SQLiteBinaries", "x64", "System.Data.SQLite.Linq.dll");
				}

				System.IO.File.Copy(sqliteFileSource, sqliteFileDest, true);
				System.IO.File.Copy(sqliteLinqFileSource, sqliteFileLinqDest, true);
			}
			catch
			{}
		}

		[TearDown]
		public void TearDown() 
		{
			_appInstance.Close();
		}

		[Test]
		public void Homepage_HasContent()
		{
			AspNetResponse homePage = _appInstance.ProcessRequest("/");

			var html = new HtmlDocument();
			html.LoadHtml(homePage.BodyAsString);

			var document = html.DocumentNode;
			var h1Elements = document.QuerySelectorAll("h1");

			Assert.That(h1Elements.Count(),Is.GreaterThan(0)); 
		}
	}
}
