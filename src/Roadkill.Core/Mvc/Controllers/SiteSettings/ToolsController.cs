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
		private IRepository _repository;
		private IPluginFactory _pluginFactory;
		private WikiExporter _wikiExporter;

		public ToolsController(ApplicationSettings settings, UserServiceBase userManager,
			SettingsService settingsService, PageService pageService, SearchService searchService, IUserContext context,
			ListCache listCache, PageViewModelCache pageViewModelCache, IWikiImporter wikiImporter, 
			IRepository repository, IPluginFactory pluginFactory, WikiExporter wikiExporter)
			: base(settings, userManager, context, settingsService) 
		{
			_settingsService = settingsService;
			_pageService = pageService;
			_searchService = searchService;
			_listCache = listCache;
			_pageViewModelCache = pageViewModelCache;
			_wikiImporter = wikiImporter;			
			_repository = repository;
			_pluginFactory = pluginFactory;
			_wikiExporter = wikiExporter;
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
		/// If an error occurs, the action adds the error message to the TempData 'ErrorMessage' item.</returns>
		public ActionResult ExportAsXml()
		{
			try
			{
				Stream stream = _wikiExporter.ExportAsXml();

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
		/// If an error occurs, the action adds the error message to the TempData 'ErrorMessage' item.</returns>
		public ActionResult ExportAsWikiFiles()
		{
			try
			{
				string zipFilename = string.Format("export-{0}.zip", DateTime.UtcNow.ToString("yyyy-MM-dd-HHmm"));
				_wikiExporter.ExportAsWikiFiles(zipFilename);

				string zipFullPath = Path.Combine(_wikiExporter.ExportFolder, zipFilename);

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
		/// If an error occurs, the action adds the error message to the TempData 'ErrorMessage' item.</returns>
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
		/// If an error occurs, the action adds the error message to the TempData 'ErrorMessage' item.</returns>
		public ActionResult ExportAsSql()
		{
			try
			{
				Stream stream = _wikiExporter.ExportAsSql();
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
			_pageService.ClearPageTables();
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

	public class WikiExporter
	{
		private readonly PageService _pageService;
		private readonly SqlExportBuilder _sqlExportBuilder;

		public string ExportFolder { get; set; }

		public WikiExporter(PageService pageService, IRepository repository, IPluginFactory pluginFactory)
		{
			if (pageService == null)
				throw new ArgumentNullException("pageService");

			_pageService = pageService;
			_sqlExportBuilder = new SqlExportBuilder(repository, pluginFactory);

			ExportFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Export");
		}

		public Stream ExportAsXml()
		{
			string xml = _pageService.ExportToXml();

			// Don't dispose the stream (as the FileStreamResult will need it open)
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(xml);
			writer.Flush();
			stream.Position = 0;

			return stream;
		}

		public Stream ExportAsSql()
		{
			string sql = _sqlExportBuilder.Export();

			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(sql);
			writer.Flush();
			stream.Position = 0;

			return stream;
		}

		public void ExportAsWikiFiles(string filename)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			IEnumerable<PageViewModel> pages = _pageService.AllPages();
			char[] invalidChars = Path.GetInvalidFileNameChars();

			if (!Directory.Exists(ExportFolder))
				Directory.CreateDirectory(ExportFolder);

			string zipFullPath = Path.Combine(ExportFolder, filename);

			using (ZipFile zip = new ZipFile(zipFullPath))
			{
				int index = 0;
				List<string> filenames = new List<string>();

				foreach (PageViewModel summary in pages.OrderBy(p => p.Title))
				{
					// Double check for blank titles, as the API can add
					// pages with blanks titles even though the UI doesn't allow it.
					if (string.IsNullOrEmpty(summary.Title))
						summary.Title = "(No title -" +summary.Id+ ")";

					string filePath = summary.Title;

					// Ensure the filename is unique as its title based.
					// Simply replace invalid path characters with a '-'
					foreach (char item in invalidChars)
					{
						filePath = filePath.Replace(item, '-');
					}

					if (filenames.Contains(filePath))
						filePath += (++index) + "";
					else
						index = 0;

					filenames.Add(filePath);

					filePath = Path.Combine(ExportFolder, filePath);
					filePath += ".wiki";
					string content = "Tags:" + summary.SpaceDelimitedTags() + "\r\n" + summary.Content;

					System.IO.File.WriteAllText(filePath, content);
					zip.AddFile(filePath, "");
				}

				zip.Save();
			}
		}
	}
}
