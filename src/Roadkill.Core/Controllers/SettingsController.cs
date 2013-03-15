using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using Ionic.Zip;
using Roadkill.Core.Search;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Controllers
{
	/// <summary>
	/// Provides functionality for the settings page including tools and user management.
	/// </summary>
	/// <remarks>All actions in this controller require admin rights.</remarks>
	[AdminRequired]
	public class SettingsController : ControllerBase
	{
		private SettingsManager _settingsManager;
		private PageManager _pageManager;
		private SearchManager _searchManager;

		public SettingsController(IConfigurationContainer configuration, UserManager userManager,
			SettingsManager settingsManager, PageManager pageManager, SearchManager searchManager, IRoadkillContext context)
			: base(configuration, userManager, context) 
		{
			_settingsManager = settingsManager;
			_pageManager = pageManager;
			_searchManager = searchManager;
		}

		/// <summary>
		/// The default settings page that displays the current Roadkill settings.
		/// </summary>
		/// <returns>A <see cref="SettingsSummary"/> as the model.</returns>
		public ActionResult Index()
		{
			SettingsSummary summary = SettingsSummary.FromSystemSettings(Configuration);
			return View(summary);
		}

		/// <summary>
		/// Saves the <see cref="SettingsSummary"/> that is POST'd to the action.
		/// </summary>
		/// <param name="summary">The settings to save to the web.config/database.</param>
		/// <returns>A <see cref="SettingsSummary"/> as the model.</returns>
		[HttpPost]
		public ActionResult Index(SettingsSummary summary)
		{
			if (ModelState.IsValid)
			{
				_settingsManager.SaveWebConfigSettings(summary);
				_settingsManager.SaveSitePreferences(summary, false);
			}
			return View(summary);
		}

		/// <summary>
		/// Displays the Users view.
		/// </summary>
		/// <returns>An <see cref="IList&lt;IEnumerable&lt;UserSummary&gt;&gt;"/> as the model. The first item contains a list of admin users,
		/// the second item contains a list of editor users. If Windows authentication is being used, the action uses the 
		/// UsersForWindows view.</returns>
		[ImportModelState]
		public ActionResult Users()
		{
			var list = new List<IEnumerable<UserSummary>>();
			list.Add(UserManager.ListAdmins());
			list.Add(UserManager.ListEditors());

			if (UserManager.IsReadonly)
				return View("UsersReadOnly", list);
			else
				return View(list);
		}

		/// <summary>
		/// Adds an admin user to the system, validating the <see cref="UserSummary"/> first.
		/// </summary>
		/// <param name="summary">The user details to add.</param>
		/// <returns>Redirects to the Users action. Additionally, if an error occurred, TempData["action"] contains the string "addadmin".</returns>
		[HttpPost]
		[ExportModelState]
		public ActionResult AddAdmin(UserSummary summary)
		{
			if (ModelState.IsValid)
			{
				UserManager.AddUser(summary.NewEmail, summary.NewUsername, summary.Password, true, false);

				// TODO
				// ModelState.AddModelError("General", errors);
			}
			else
			{
				// Instructs the view to reshow the modal dialog
				TempData["action"] = "addadmin";
			}

			return RedirectToAction("Users");
		}

		/// <summary>
		/// Adds an editor user to the system, validating the <see cref="UserSummary"/> first.
		/// </summary>
		/// <param name="summary">The user details to add.</param>
		/// <returns>Redirects to the Users action. Additionally, if an error occurred, TempData["action"] contains the string "addeditor".</returns>
		[HttpPost]
		[ExportModelState]
		public ActionResult AddEditor(UserSummary summary)
		{
			if (ModelState.IsValid)
			{
				try
				{
					UserManager.AddUser(summary.NewEmail, summary.NewUsername, summary.Password, false, true);
				}
				catch (SecurityException e)
				{
					ModelState.AddModelError("General", e.Message);
				}
			}
			else
			{
				// Instructs the view to reshow the modal dialog
				TempData["action"] = "addeditor";
			}

			return RedirectToAction("Users");
		}

		/// <summary>
		/// Edits an existing user. If the <see cref="UserSummary.Password"/> property is not blank, the password
		/// for the user is reset and then changed.
		/// </summary>
		/// <param name="summary">The user details to edit.</param>
		/// <returns>Redirects to the Users action. Additionally, if an error occurred, TempData["edituser"] contains the string "addeditor".</returns>
		[HttpPost]
		[ExportModelState]
		public ActionResult EditUser(UserSummary summary)
		{
			if (ModelState.IsValid)
			{
				if (summary.UsernameHasChanged || summary.EmailHasChanged)
				{
					if (!UserManager.UpdateUser(summary))
					{
						ModelState.AddModelError("General", SiteStrings.SiteSettings_Users_EditUser_Error);
					}

					summary.ExistingEmail = summary.NewEmail;
				}

				if (!string.IsNullOrEmpty(summary.Password))
					UserManager.ChangePassword(summary.ExistingEmail, summary.Password);
			}
			else
			{
				// Instructs the view to reshow the modal dialog
				TempData["action"] = "edituser";
			}

			return RedirectToAction("Users");
		}

		/// <summary>
		/// Removes a user from the system.
		/// </summary>
		/// <param name="id">The id of the user to remove.</param>
		/// <returns>Redirects to the Users action.</returns>
		public ActionResult DeleteUser(string id)
		{
			UserManager.DeleteUser(id);
			return RedirectToAction("Users");
		}

		/// <summary>
		/// Displays the tools page.
		/// </summary>
		public ActionResult Tools()
		{
			return View();
		}

		/// <summary>
		/// Exports the pages of site including their history as a single XML file.
		/// </summary>
		/// <returns>A <see cref="FileStreamResult"/> called 'roadkill-export.xml' containing the XML data.
		/// If an error occurs, a <see cref="HttpNotFound"/> result is returned and the error message written to the trace.</returns>
		public ActionResult ExportAsXml()
		{
			try
			{
				string xml = _pageManager.ExportToXml();

				// Let the FileStreamResult dispose the stream
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
				Log.Warn(e, "Unable to export as XML");
				TempData["Message"] = string.Format(SiteStrings.SiteSettings_Tools_ExportXml_Error, e.Message);

				return RedirectToAction("Tools");
			}
		}

		/// <summary>
		/// Exports the pages of the site as .wiki files, in ZIP format.
		/// </summary>
		/// <returns>A <see cref="FileStreamResult"/> called 'export-{date}.zip'. This file is saved in the App_Data folder first.
		/// If an error occurs, a <see cref="HttpNotFound"/> result is returned and the error message written to the trace.</returns>
		/// </returns>
		public ActionResult ExportAsWikiFiles()
		{
			IEnumerable<PageSummary> pages = _pageManager.AllPages();

			try
			{
				string exportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"\App_Data", "export");
				Directory.CreateDirectory(exportFolder);

				string zipFilename = string.Format("export-{0}.zip", DateTime.Now.ToString("yyyy-MM-dd-HHmm"));
				string zipFullPath = Path.Combine(exportFolder, zipFilename);
				using (ZipFile zip = new ZipFile(zipFullPath))
				{
					int index = 0;
					List<string> filenames = new List<string>();

					foreach (PageSummary summary in pages.OrderBy(p => p.Title))
					{
						// Ensure the filename is unique as its title based.
						string filePath = summary.Title.AsValidFilename();
						if (filenames.Contains(filePath))
							filePath += (++index) + "";
						else
							index = 0;

						filenames.Add(filePath);

						filePath = Path.Combine(exportFolder, filePath);
						filePath += ".wiki";
						string content = "Tags:" + summary.SpaceDelimitedTags() + "\r\n" + summary.Content;

						System.IO.File.WriteAllText(filePath, content);
						zip.AddFile(filePath, "");
					}

					zip.Save();
				}

				return File(zipFullPath, "application/zip", zipFilename);
			}
			catch (IOException e)
			{
				Log.Warn(e, "Unable to export wiki content");
				TempData["Message"] = string.Format(SiteStrings.SiteSettings_Tools_ExportContent_Error, e.Message);

				return RedirectToAction("Tools");
			}
		}

		/// <summary>
		/// Exports the Attachments folder contents (including subdirectories) in ZIP format.
		/// </summary>
		/// <returns>A <see cref="FileStreamResult"/> called 'attachments-export-{date}.zip'. This file is saved in the App_Data folder first.
		/// If an error occurs, a <see cref="HttpNotFound"/> result is returned and the error message written to the trace.</returns>
		/// </returns>
		public ActionResult ExportAttachments()
		{
			IEnumerable<PageSummary> pages = _pageManager.AllPages();

			try
			{
				string exportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"\App_Data", "export");
				Directory.CreateDirectory(exportFolder);

				string zipFilename = string.Format("attachments-export-{0}.zip", DateTime.Now.ToString("yyy-MM-dd-HHss"));
				string zipFullPath = Path.Combine(exportFolder, zipFilename);
				using (ZipFile zip = new ZipFile(zipFullPath))
				{
					zip.AddDirectory(Configuration.ApplicationSettings.AttachmentsFolder, "Attachments");
					zip.Save();
				}

				return File(zipFullPath, "application/zip", zipFilename);
			}
			catch (IOException e)
			{
				Log.Warn(e, "Unable to export attachments");
				TempData["Message"] = string.Format(SiteStrings.SiteSettings_Tools_ExportAttachments_Error, e.Message);

				return RedirectToAction("Tools");
			}
		}

		/// <summary>
		/// Attempts to import page data and files from a Screwturn wiki database.
		/// </summary>
		/// <param name="screwturnConnectionString">The connection string to the Screwturn database.</param>
		/// <returns>Redirects to the Tools action.</returns>
		[HttpPost]
		public ActionResult ImportFromScrewTurn(string screwturnConnectionString)
		{
			ScrewTurnImporter importer = new ScrewTurnImporter(Configuration);
			importer.ImportFromSql(screwturnConnectionString);
			TempData["Message"] = SiteStrings.SiteSettings_Tools_ScrewTurnImport_Message;

			return RedirectToAction("Tools");
		}

		/// <summary>
		/// Deletes and re-creates the search index.
		/// </summary>
		/// <returns>Redirects to the Tools action.</returns>
		public ActionResult UpdateSearchIndex()
		{
			TempData["Message"] = SiteStrings.SiteSettings_Tools_RebuildSearch_Message;
			_searchManager.CreateIndex();
			return RedirectToAction("Tools");
		}

		/// <summary>
		/// Clears all wiki pages from the database.
		/// </summary>
		/// <returns>Redirects to the Tools action.</returns>
		public ActionResult ClearPages()
		{
			TempData["Message"] = SiteStrings.SiteSettings_Tools_ClearDatabase_Message;
			_settingsManager.ClearPageTables();
			return RedirectToAction("Tools");
		}

		/// <summary>
		/// Renames a tag in the system, and updates all pages that use it.
		/// </summary>
		/// <returns>Redirects to the Tools action.</returns>
		public ActionResult RenameTag(string oldTagName, string newTagName)
		{
			TempData["Message"] = SiteStrings.SiteSettings_Tools_RenameTag_Message;

			_pageManager.RenameTag(oldTagName, newTagName);

			return RedirectToAction("Tools");
		}

		/// <summary>
		/// The default settings page that displays the current Roadkill settings.
		/// </summary>
		/// <returns>A <see cref="SettingsSummary"/> as the model.</returns>
		public ActionResult SitePreferences()
		{
			Configuration.SitePreferences.GetJson();
			return Content(Configuration.SitePreferences.GetJson(), "text/json");
		}
	}
}
