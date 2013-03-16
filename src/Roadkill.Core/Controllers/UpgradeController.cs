using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides an automated way of upgrading from a previous version of Roadkill.
	/// </summary>
	public class UpgradeController : ControllerBase
	{
		private IRepository _repository;
		private SettingsManager _settingsManager;

		public UpgradeController(IConfigurationContainer configuration, IRepository repository, UserManager userManager, IRoadkillContext context, SettingsManager settingsManager)
			: base (configuration, userManager, context)
		{
			_repository = repository;
			_settingsManager = settingsManager;
		}

		public ActionResult Index()
		{
			if (!Configuration.ApplicationSettings.UpgradeRequired)
				return RedirectToAction("Index", "Home");

			return View();
		}

		[HttpPost]
		public ActionResult Run()
		{
			if (!Configuration.ApplicationSettings.UpgradeRequired)
				return RedirectToAction("Index", "Home");

			try
			{
				_repository.Upgrade(Configuration);
				_settingsManager.SaveCurrentVersionToWebConfig();

				return RedirectToAction("Index", "Home");
			}
			catch (UpgradeException ex)
			{
				return View("Index", ex);
			}
		}
	}
}
