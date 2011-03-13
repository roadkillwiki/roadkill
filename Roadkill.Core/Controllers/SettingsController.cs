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

namespace Roadkill.Core.Controllers
{
	[AdminRequired]
	public class SettingsController : ControllerBase
    {
		/// <summary>
		/// TODO
		/// </summary>
		/// <returns></returns>
		public ActionResult Index()
		{
			return View();
		}

		public ActionResult AddAdmin(string username)
		{
			UserManager manager = new UserManager();
			manager.AddAdminUser(username, "password");

			return RedirectToAction("Users");
		}

		/// <summary>
		/// TEST ONLY. Remove when live + add as tests
		/// </summary>
		/// <returns></returns>
		public ActionResult AddAdmins()
		{
			UserManager manager = new UserManager();
			manager.AddAdminUser("chris", "password");
			manager.AddAdminUser("chris2", "password");
			manager.AddAdminUser("chris3", "password");
			manager.AddAdminUser("chris4", "password");

			return RedirectToAction("Users");
		}

		public ActionResult AddEditors()
		{
			UserManager manager = new UserManager();
			manager.AddUser("editor1", "password");
			manager.AddUser("editor2", "password");
			manager.AddUser("editor3", "password");
			manager.AddUser("editor4", "password");

			return RedirectToAction("Users");
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

		public ActionResult Delete(string id)
		{
			UserManager manager = new UserManager();
			manager.DeleteUser(id);


			return RedirectToAction("Users");
		}

		[HttpPost]
		public ActionResult Users(string mode, string userType, string username, string newUsername, string password, string passwordConfirm)
		{
			if (string.IsNullOrEmpty(newUsername))
				ModelState.AddModelError("Username", "The username is blank");

			if (!string.IsNullOrEmpty(password))
			{
				if (string.IsNullOrEmpty(passwordConfirm))
				{
					ModelState.AddModelError("Password", "Confirm your password");
				}
				else if (password != passwordConfirm)
				{
					ModelState.AddModelError("Password", "The passwords don't match.");
				}
			}

			UserManager manager = new UserManager();

			if (ModelState.IsValid)
			{
				if (mode == "new")
				{
					string errors = "";
					if (userType == "admin")
						errors = manager.AddAdminUser(newUsername, password);
					else
						errors = manager.AddUser(newUsername, password);

					if (!string.IsNullOrEmpty(errors))
					{
						ViewData["IsValid"] = "false";
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

					if (!string.IsNullOrEmpty(password))
						manager.UpdateUser(username, password, username);
				}
			}
			else
			{
				ViewData["IsValid"] = "false";
			}

			//
			// Don't RedirectToAction as we lose the model state.
			//
			IList<IEnumerable<string>> list = new List<IEnumerable<string>>();
			list.Add(manager.AllAdmins());
			list.Add(manager.AllEditors());

			return View(list);
		}

		public ActionResult Tools()
		{
			return View();
		}

		public ActionResult WipePages()
		{
			Page.Configure(RoadkillSettings.ConnectionString, true,RoadkillSettings.CachedEnabled);
			return RedirectToAction("Index", "Home");
		}

		public ActionResult ExportAsXml()
		{
			PageManager manager = new PageManager();
			string xml = manager.ExportToXml();

			try
			{
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

		[HttpPost]
		public ActionResult ImportFromScrewTurn(string screwturnConnectionString)
		{
			ScrewTurnImporter importer = new ScrewTurnImporter();
			importer.ImportFromSql(screwturnConnectionString);
			TempData["DoneMessage"] = "Import successful";

			return RedirectToAction("Tools");
		}

		public ActionResult ExportContent()
		{
			PageManager manager = new PageManager();
			IEnumerable<PageSummary> pages = manager.AllPages();

			try
			{
				string exportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"__export" + DateTime.UtcNow.Ticks);
				Directory.CreateDirectory(exportFolder);

				string zipFilename = string.Format("export-{0}.zip", DateTime.Now.ToString("dd-MM-yyyy"));
				string zipFullPath = Path.Combine(exportFolder,zipFilename);
				using (ZipFile zip = new ZipFile(zipFullPath))
				{
					
					foreach (PageSummary summary in pages)
					{
						string filePath = Path.Combine(exportFolder,summary.Title.AsValidFilename() + ".wiki");
						string content = "Tags:" + summary.Tags.SpaceDelimitTags() + "\r\n" + summary.Content;

						System.IO.File.WriteAllText(filePath, content);
						zip.AddFile(filePath,"");
					}

					zip.Save();
				}

				//
				// Cleanup - delete all files in the temp export folder, remove the folder
				//
				try
				{
					//Directory.Delete(exportFolder, true);
				}
				catch(IOException e)
				{
					// Log
					Trace.Write(string.Format("Unable to delete temporary export folder {0}: {1}", exportFolder, e.Message));
				}

				return File(zipFullPath, "application/zip", zipFullPath);			
			}
			catch (IOException e)
			{
				Trace.Write(string.Format("Unable to export files: {0}", e));
				return HttpNotFound("There was a problem with the export. Enable tracing to see the error source");
			}
		}
    }
}
