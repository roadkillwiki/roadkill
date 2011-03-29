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
using Ionic.Zip;
using Ionic.Zlib;
using System.Diagnostics;
using Roadkill.Core.Search;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality for various settings and tools.
	/// </summary>
	/// <remarks>All actions in this controller require admin rights.</remarks>
	[AdminRequired]
	public class SettingsController : ControllerBase
    {
		public ActionResult Index()
		{
			SettingsSummary summary = SettingsSummary.GetCurrentSettings();
			return View(summary);
		}

		[HttpPost]
		public ActionResult Index(SettingsSummary summary)
		{
			if (ModelState.IsValid)
			{
				InstallManager.SaveWebConfigSettings(summary);
				InstallManager.SaveDbSettings(summary, false);
			}
			return View(summary);
		}

		public ActionResult AddAdmin(string username)
		{
			UserManager manager = new UserManager();
			manager.AddAdminUser(username, "password");

			return RedirectToAction("Users");
		}

		public ActionResult DeleteUser(string id)
		{
			UserManager manager = new UserManager();
			manager.DeleteUser(id);

			return RedirectToAction("Users");
		}

		public ActionResult ExportAsXml()
		{
			try
			{

				PageManager manager = new PageManager();
				string xml = manager.ExportToXml();

				// Let the FileStreamResult dispose
				MemoryStream stream = new MemoryStream();
				StreamWriter writer = new StreamWriter(stream);
				writer.Write(xml);
				writer.Flush();
				stream.Position = 0;

				FileStreamResult result = new FileStreamResult(stream, "text/xml");
				result.FileDownloadName = "roadkill-export.xml";

				return result;
			}
			catch (IOException e)
			{
				Trace.Write(string.Format("Unable to export as XML: {0}", e));
				return HttpNotFound("There was a problem with exporting as XML. Enable tracing to see the error source");
			}
		}

		public ActionResult ExportContent()
		{
			PageManager manager = new PageManager();
			IEnumerable<PageSummary> pages = manager.AllPages();

			try
			{
				string exportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"\App_Data", "export");
				Directory.CreateDirectory(exportFolder);

				string zipFilename = string.Format("export-{0}.zip", DateTime.Now.ToString("yyyy-MM-dd-HHmm"));
				string zipFullPath = Path.Combine(exportFolder, zipFilename);
				using (ZipFile zip = new ZipFile(zipFullPath))
				{

					foreach (PageSummary summary in pages)
					{
						string filePath = Path.Combine(exportFolder, summary.Title.AsValidFilename() + ".wiki");
						string content = "Tags:" + summary.Tags.SpaceDelimitTags() + "\r\n" + summary.Content;

						System.IO.File.WriteAllText(filePath, content);
						zip.AddFile(filePath, "");
					}

					zip.Save();
				}

				return File(zipFullPath, "application/zip", zipFullPath);
			}
			catch (IOException e)
			{
				Trace.Write(string.Format("Unable to export files: {0}", e));
				return HttpNotFound("There was a problem with the export. Enable tracing to see the error source");
			}
		}

		public ActionResult ExportAttachments()
		{
			PageManager manager = new PageManager();
			IEnumerable<PageSummary> pages = manager.AllPages();

			try
			{
				string exportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory +@"\App_Data", "export");
				Directory.CreateDirectory(exportFolder);

				string zipFilename = string.Format("attachments-export-{0}.zip", DateTime.Now.ToString("yyy-MM-dd-HHss"));
				string zipFullPath = Path.Combine(exportFolder, zipFilename);
				using (ZipFile zip = new ZipFile(zipFullPath))
				{
					zip.AddDirectory(Server.MapPath(RoadkillSettings.AttachmentsFolder), "Attachments");
					zip.Save();
				}

				return File(zipFullPath, "application/zip", zipFullPath);
			}
			catch (IOException e)
			{
				Trace.Write(string.Format("Unable to export files: {0}", e));
				return HttpNotFound("There was a problem with the attachments export. Enable tracing to see the error source");
			}
		}

		[HttpPost]
		public ActionResult ImportFromScrewTurn(string screwturnConnectionString)
		{
			ScrewTurnImporter importer = new ScrewTurnImporter();
			importer.ImportFromSql(screwturnConnectionString);
			TempData["Message"] = "Import successful";

			return RedirectToAction("Tools");
		}

		public ActionResult Tools()
		{
			return View();
		}

		public ActionResult UpdateSearchIndex()
		{
			TempData["Message"] = "Update complete";
			SearchManager.Current.CreateIndex();
			return RedirectToAction("Tools");
		}

		public ActionResult Users()
		{
			UserManager manager = new UserManager();
			IList<IEnumerable<string>> list = new List<IEnumerable<string>>();
			list.Add(manager.AllAdmins());
			list.Add(manager.AllEditors());

			if (RoadkillSettings.IsWindowsAuthentication)
				return View("UsersForWindows", list);
			else
				return View(list);
		}

		[HttpPost]
		public ActionResult Users(string mode, string userType, string username, string newUsername, string passwordMain, string passwordConfirm)
		{
			if (string.IsNullOrEmpty(newUsername))
				ModelState.AddModelError("Username", "The username is blank");

			// Refactor this into a model
			if (!string.IsNullOrEmpty(passwordMain))
			{
				if (string.IsNullOrEmpty(passwordConfirm))
				{
					ModelState.AddModelError("Password", "Confirm your password");
				}
				else if (passwordMain != passwordConfirm)
				{
					ModelState.AddModelError("Password", "The passwords don't match.");
				}
				else if (passwordMain.Length < Membership.MinRequiredPasswordLength)
				{
					ModelState.AddModelError("Password", string.Format("The password is less than {0} characters", Membership.MinRequiredPasswordLength));
				}
			}

			UserManager manager = new UserManager();

			if (ModelState.IsValid)
			{
				if (mode == "new")
				{
					string errors = "";
					if (userType == "admin")
						errors = manager.AddAdminUser(newUsername, passwordMain);
					else
						errors = manager.AddUser(newUsername, passwordMain);

					if (!string.IsNullOrEmpty(errors))
					{
						ModelState.AddModelError("General", errors);
					}
				}
				else
				{

					if (username != newUsername)
					{
						manager.ChangeUsername(username, newUsername);
						username = newUsername;
					}

					if (!string.IsNullOrEmpty(passwordMain))
						manager.ChangePassword(username, passwordMain, username);
				}
			}

			//
			// Don't RedirectToAction as we lose the model state.
			//
			IList<IEnumerable<string>> list = new List<IEnumerable<string>>();
			list.Add(manager.AllAdmins());
			list.Add(manager.AllEditors());

			return View(list);
		}

		public ActionResult WipePages()
		{
			TempData["Message"] = "Database cleared";
			InstallManager.ClearPageTables(RoadkillSettings.ConnectionString);
			return RedirectToAction("Tools");
		}
    }
}
