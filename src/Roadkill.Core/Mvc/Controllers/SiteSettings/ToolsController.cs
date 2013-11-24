using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using Ionic.Zip;
using Roadkill.Core.Localization;
using Roadkill.Core.Configuration;
using Roadkill.Core.Cache;
using Roadkill.Core.Services;
using Roadkill.Core.Import;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.Attributes;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Logging;
using Roadkill.Core.Database.Export;
using Roadkill.Core.Database;
using Roadkill.Core.Plugins;

namespace Roadkill.Core.Mvc.Controllers
{
	/// <summary>
	/// Provides functionality for the tools page for admins.
	/// </summary>
	/// <remarks>All actions in this controller require admin rights.</remarks>
	[AdminRequired]
	public class ToolsController : ControllerBase
	{
		private SettingsService _settingsService;
		private PageService _pageService;
		private SearchService _searchService;
		private IWikiImporter _wikiImporter;
		private ListCache _listCache;
		private PageViewModelCache _pageViewModelCache;
		private SiteCache _siteCache;
		private IRepository _repository;
		private IPluginFactory _pluginFactory;

		public ToolsController(ApplicationSettings settings, UserServiceBase userManager,
			SettingsService settingsService, PageService pageService, SearchService searchService, IUserContext context,
			ListCache listCache, PageViewModelCache pageViewModelCache, SiteCache siteCache, IWikiImporter wikiImporter, 
			IRepository repository, IPluginFactory pluginFactory)
			: base(settings, userManager, context, settingsService) 
		{
			_settingsService = settingsService;
			_pageService = pageService;
			_searchService = searchService;
			_listCache = listCache;
			_pageViewModelCache = pageViewModelCache;
			_siteCache = siteCache;
			_wikiImporter = wikiImporter;			
			_repository = repository;
			_pluginFactory = pluginFactory;
		}

		/// <summary>
		/// Displays the main tools page.
		/// </summary>
		public ActionResult Index()
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
				string xml = _pageService.ExportToXml();

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
				TempData["ErrorMessage"] = string.Format(SiteStrings.SiteSettings_Tools_ExportXml_Error, e.Message);

				return RedirectToAction("Index");
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
			IEnumerable<PageViewModel> pages = _pageService.AllPages();

			try
			{
				string exportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Export");
				Directory.CreateDirectory(exportFolder);

				string zipFilename = string.Format("export-{0}.zip", DateTime.UtcNow.ToString("yyyy-MM-dd-HHmm"));
				string zipFullPath = Path.Combine(exportFolder, zipFilename);
				using (ZipFile zip = new ZipFile(zipFullPath))
				{
					int index = 0;
					List<string> filenames = new List<string>();

					foreach (PageViewModel summary in pages.OrderBy(p => p.Title))
					{
						// Ensure the filename is unique as its title based.
						// Simply replace invalid path characters with a '-'
						string filePath = summary.Title;
						char[] invalidChars = Path.GetInvalidFileNameChars();
						foreach (char item in invalidChars)
						{
							filePath = filePath.Replace(item, '-');
						}

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
				TempData["ErrorMessage"] = string.Format(SiteStrings.SiteSettings_Tools_ExportContent_Error, e.Message);

				return RedirectToAction("Index");
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
			IEnumerable<PageViewModel> pages = _pageService.AllPages();

			try
			{
				string exportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Export");
				Directory.CreateDirectory(exportFolder);

				string zipFilename = string.Format("attachments-export-{0}.zip", DateTime.UtcNow.ToString("yyy-MM-dd-HHss"));
				string zipFullPath = Path.Combine(exportFolder, zipFilename);
				using (ZipFile zip = new ZipFile(zipFullPath))
				{
					zip.AddDirectory(ApplicationSettings.AttachmentsDirectoryPath, "Attachments");
					zip.Save();
				}

				return File(zipFullPath, "application/zip", zipFilename);
			}
			catch (IOException e)
			{
				Log.Warn(e, "Unable to export attachments");
				TempData["ErrorMessage"] = string.Format(SiteStrings.SiteSettings_Tools_ExportAttachments_Error, e.Message);

				return RedirectToAction("Index");
			}
		}

		/// <summary>
		/// Exports the roadkill database (User, Page, PageContent) as a SQL script.
		/// </summary>
		/// <returns>A <see cref="FileStreamResult"/> called 'roadkill-export.sql' containing the SQL data.
		/// If an error occurs, a <see cref="HttpNotFound"/> result is returned and the error message written to the trace.</returns>
		public ActionResult ExportAsSql()
		{
			try
			{
				SqlExportBuilder scriptBuilder = new SqlExportBuilder(_repository, _pluginFactory);
				string sql = scriptBuilder.Export();

				// Let the FileStreamResult dispose the stream
				MemoryStream stream = new MemoryStream();
				StreamWriter writer = new StreamWriter(stream);
				writer.Write(sql);
				writer.Flush();
				stream.Position = 0;

				FileStreamResult result = new FileStreamResult(stream, "text/plain");
				result.FileDownloadName = "roadkill-export.sql";

				return result;
			}
			catch (IOException e)
			{
				Log.Warn(e, "Unable to export as SQL");
				TempData["ErrorMessage"] = string.Format(SiteStrings.SiteSettings_Tools_ExportXml_Error, e.Message);

				return RedirectToAction("Index");
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
			string message = "";

			if (string.IsNullOrEmpty(screwturnConnectionString))
			{
				TempData["ErrorMessage"] = "Please enter a Screwturn connection string";
			}
			else
			{
				_wikiImporter.ImportFromSqlServer(screwturnConnectionString);
				_wikiImporter.UpdateSearchIndex(_searchService);
				TempData["SuccessMessage"] = SiteStrings.SiteSettings_Tools_ScrewTurnImport_Message;

			}
			
			return RedirectToAction("Index");
		}

		/// <summary>
		/// Deletes and re-creates the search index.
		/// </summary>
		/// <returns>Redirects to the Tools action.</returns>
		public ActionResult UpdateSearchIndex()
		{
			TempData["SuccessMessage"] = SiteStrings.SiteSettings_Tools_RebuildSearch_Message;
			_searchService.CreateIndex();

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Clears all wiki pages from the database.
		/// </summary>
		/// <returns>Redirects to the Tools action.</returns>
		public ActionResult ClearPages()
		{
			TempData["SuccessMessage"] = SiteStrings.SiteSettings_Tools_ClearDatabase_Message;
			_settingsService.ClearPageTables();
			_listCache.RemoveAll();
			_pageViewModelCache.RemoveAll();

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Renames a tag in the system, and updates all pages that use it.
		/// </summary>
		/// <returns>Redirects to the Tools action.</returns>
		public ActionResult RenameTag(string oldTagName, string newTagName)
		{
			TempData["SuccessMessage"] = SiteStrings.SiteSettings_Tools_RenameTag_Message;
			_pageService.RenameTag(oldTagName, newTagName);

			return RedirectToAction("Index");
		}

		/// <summary>
		/// Displays the SiteSettings Json content.
		/// </summary>
		/// <returns></returns>
		public ActionResult SiteSettings()
		{
			return Content(SettingsService.GetSiteSettings().GetJson(), "text/json");
		}
	}
}
