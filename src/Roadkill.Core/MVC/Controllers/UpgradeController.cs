using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides an automated way of upgrading from a previous version of Roadkill.
	/// </summary>
	public class UpgradeController : ControllerBase
	{
		private IRepository _repository;

		public UpgradeController(ApplicationSettings settings, IRepository repository, UserManagerBase userManager,
			IUserContext context, SettingsManager siteSettingsManager)
			: base (settings, userManager, context, siteSettingsManager)
		{
			_repository = repository;
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
				ConfigFileManager manager = new ConfigFileManager();
				manager.WriteCurrentVersionToWebConfig();
				manager.Save();

				return RedirectToAction("Index", "Home");
			}
			catch (UpgradeException ex)
			{
				return View("Index", ex);
			}
		}
	}
}
