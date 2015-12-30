﻿using System;
using System.Web.Mvc;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;
using Roadkill.Core.Security.Windows;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Contains AJAX actions for testing various configuration options: database connections, attachments.
	/// This controller is only accessable by admins, if the installed state is false.
	/// </summary>
	public class ConfigurationTesterController : Controller // Don't inherit from ControllerBase, as it checks if Installed is true.
	{
		private readonly ApplicationSettings _applicationSettings;
		private readonly IUserContext _userContext;
		private readonly ConfigReaderWriter _configReaderWriter;
		private readonly IActiveDirectoryProvider _activeDirectoryProvider;
		private readonly UserServiceBase _userService;
		private readonly IDatabaseTester _databaseTester;

		public ConfigurationTesterController(ApplicationSettings appSettings, IUserContext userContext, ConfigReaderWriter configReaderWriter, 
			IActiveDirectoryProvider activeDirectoryProvider, UserServiceBase userService, IDatabaseTester databaseTester) 
		{
			_applicationSettings = appSettings;
			_userContext = userContext;
			_configReaderWriter = configReaderWriter;
			_activeDirectoryProvider = activeDirectoryProvider;
			_userService = userService;
			_databaseTester = databaseTester;
		}

		protected override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			_userContext.CurrentUser = _userService.GetLoggedInUserName(HttpContext);
			ViewBag.Context = _userContext;
			ViewBag.Config = _applicationSettings;
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts to contact an Active Directory server using the
		/// connection string and user details provided.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestLdap(string connectionString, string username, string password, string groupName)
		{
			if (IsInstalledAndUserIsNotAdmin())
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
			if (IsInstalledAndUserIsNotAdmin())
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
			if (IsInstalledAndUserIsNotAdmin())
				return Content("");

			string errors = AttachmentPathUtil.AttachmentFolderExistsAndWriteable(folder, HttpContext);
			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// This action is for JSON calls only. Attempts a database connection using the provided connection string.
		/// </summary>
		/// <returns>Returns a <see cref="TestResult"/> containing information about any errors.</returns>
		public ActionResult TestDatabaseConnection(string connectionString, string databaseName)
		{
			if (IsInstalledAndUserIsNotAdmin())
				return Content("");

			string errors = "";
			try
			{
				_databaseTester.TestConnection(databaseName, connectionString);
			}
			catch (Exception e)
			{
				errors = e.ToString();
			}

			return Json(new TestResult(errors), JsonRequestBehavior.AllowGet);
		}

		internal bool IsInstalledAndUserIsNotAdmin()
		{
			return _applicationSettings.Installed && !_userContext.IsAdmin;
		}
	}
}
