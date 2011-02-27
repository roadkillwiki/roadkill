using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Web.Security;
using System.IO;
using System.Web.Configuration;
using System.Configuration;

namespace Roadkill.Core.Controllers
{
	public class AdminController : ControllerBase
    {
		// - Export
		// - Re-create schema
		// - Clear all users

		public ActionResult CreateSchema()
		{
			Page.Configure(RoadkillSettings.ConnectionString, true);
			return RedirectToAction("Index", "Home");
		}

		public ActionResult Install()
		{
			if (RoadkillSettings.Installed)
				return RedirectToAction("Index", "Home");

			return View();
		}

		[HttpPost]
		public ActionResult Install(string connectionString,string adminPassword)
		{
			UserManager manager = new UserManager();

			MembershipCreateStatus status = manager.AddAdminUser(adminPassword, "admin@localhost");
			if (status == MembershipCreateStatus.DuplicateUserName)
			{
				// Do nothing, for now. The passwords may be out of sync which 
				// requires the view being changed to accomodate this.
			}

			Page.Configure(RoadkillSettings.ConnectionString, true);
			RoadkillSettings.Install(connectionString, adminPassword);

			return View("InstallComplete");
		}

		[RoadkillAuthorize(Roles = "Admins")]
		public ActionResult Export()
		{
			PageManager manager = new PageManager();
			string xml = manager.ExportToXml();

			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(xml);
			writer.Flush();
			stream.Position = 0;

			FileStreamResult result = new FileStreamResult(stream, "text/xml");
			result.FileDownloadName = "roadkill-export.xml";

			return result;
		}
    }
}
