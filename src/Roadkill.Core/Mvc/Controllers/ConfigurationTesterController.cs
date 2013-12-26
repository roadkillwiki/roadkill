using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.DI;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Contains AJAX actions for testing various configuration options: database connections, attachments.
	/// This controller is only accessable by admins, if the installed state is false.
	/// </summary>
	public class ConfigurationTesterController : Controller // Don't inherit from ControllerBase, as it checks if Installed is true.
	{
		private ApplicationSettings _applicationSettings;
		private IUserContext _userContext;
		private ConfigReaderWriter _configReaderWriter;
		private IActiveDirectoryProvider _activeDirectoryProvider;

		public ConfigurationTesterController(ApplicationSettings appSettings, IUserContext userContext, ConfigReaderWriter configReaderWriter, IActiveDirectoryProvider activeDirectoryProvider) 
		{
			_applicationSettings = appSettings;
			_userContext = userContext;
			_configReaderWriter = configReaderWriter;
			_activeDirectoryProvider = activeDirectoryProvider;
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts to contact an Active Directory server using the
		/// connection string and user details provided.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestLdap(string connectionString, string username, string password, string groupName)
		{
			if (InstalledAndUserIsNotAdmin())
				return Content("");

			string errors = _activeDirectoryProvider.TestLdapConnection(connectionString, username, password, groupName);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts to write to the web.config file and save it.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestWebConfig()
		{
			if (InstalledAndUserIsNotAdmin())
				return Content("");

			string errors = _configReaderWriter.TestSaveWebConfig();
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Checks to see if the provided folder exists and if it can be written to.
		/// </summary>
		/// <param name="folder"></param>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestAttachments(string folder)
		{
			if (InstalledAndUserIsNotAdmin())
				return Content("");

			string errors = AttachmentPathUtil.AttachmentFolderExistsAndWriteable(folder, HttpContext);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts a database connection using the provided connection string.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestDatabaseConnection(string connectionString, string databaseType)
		{
			if (InstalledAndUserIsNotAdmin())
				return Content("");

			string errors = RepositoryManager.TestDbConnection(connectionString, databaseType);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// Attempts to copy the correct SQL binaries to the bin folder for the architecture the app pool is running under.
		/// </summary>
		public ActionResult CopySqlite()
		{
			if (InstalledAndUserIsNotAdmin())
				return Content("");

			string errors = "";

			try
			{
				string sqliteInteropFileSource = Path.Combine(_applicationSettings.SQLiteBinariesPath, "x86", "SQLite.Interop.dll");
				string sqliteInteropFileDest = Server.MapPath("~/bin/SQLite.Interop.dll");

				if (Environment.Is64BitOperatingSystem && Environment.Is64BitProcess)
				{
					sqliteInteropFileSource = Path.Combine(_applicationSettings.SQLiteBinariesPath, "x64", "SQLite.Interop.dll");
				}

				System.IO.File.Copy(sqliteInteropFileSource, sqliteInteropFileDest, true);
			}
			catch (Exception e)
			{
				errors = e.ToString();
			}

			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		internal bool InstalledAndUserIsNotAdmin()
		{
			return _applicationSettings.Installed && !_userContext.IsAdmin;
		}
	}
}
