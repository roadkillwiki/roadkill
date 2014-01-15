using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Services;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides an automated way of upgrading from a previous version of Roadkill.
	/// </summary>
	public class UpgradeController : ControllerBase
	{
		private IRepository _repository;
		private ConfigReaderWriter _configReaderWriter;

		public UpgradeController(ApplicationSettings settings, IRepository repository, UserServiceBase userService,
			IUserContext context, SettingsService settingsService, ConfigReaderWriter configReaderWriter)
			: base (settings, userService, context, settingsService)
		{
			_repository = repository;
			_configReaderWriter = configReaderWriter;
		}

		public ActionResult Index()
		{
			if (!ApplicationSettings.UpgradeRequired)
				return RedirectToAction("Index", "Home");

			return View();
		}

		[HttpPost]
		public ActionResult Run()
		{
			if (!ApplicationSettings.UpgradeRequired)
				return RedirectToAction("Index", "Home");

			try
			{
				_repository.Upgrade(ApplicationSettings);
				_configReaderWriter.UpdateCurrentVersion(ApplicationSettings.ProductVersion.ToString());

				return RedirectToAction("Index", "Home");
			}
			catch (UpgradeException ex)
			{
				return View("Index", ex);
			}
		}
	}
}
