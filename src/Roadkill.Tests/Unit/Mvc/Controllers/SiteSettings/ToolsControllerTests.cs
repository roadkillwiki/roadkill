using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Import;
using Roadkill.Core.Localization;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class ToolsControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private IWikiImporter _wikiImporter;
		private PluginFactoryMock _pluginFactory;
		private SearchService _searchService;
		private SettingsService _settingsService;
		private PageViewModelCache _pageCache;
		private ListCache _listCache;
		private SiteCache _siteCache;
		private MemoryCache _cache;
		private WikiExporter _wikiExporter;

		private ToolsController _toolsController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_pageCache = _container.PageViewModelCache;
			_listCache = _container.ListCache;
			_siteCache = _container.SiteCache;
			_cache = _container.MemoryCache;

			_pageService = _container.PageService;
			_wikiImporter = new ScrewTurnImporter(_applicationSettings, _repository);
			_pluginFactory = _container.PluginFactory;
			_searchService = _container.SearchService;

			_wikiExporter = new WikiExporter(_pageService, _repository, _pluginFactory);
			_wikiExporter.ExportFolder = AppDomain.CurrentDomain.BaseDirectory;

			_toolsController = new ToolsController(_applicationSettings, _userService, _settingsService, _pageService,
													_searchService, _context, _listCache, _pageCache, _wikiImporter, 
													_repository, _pluginFactory, _wikiExporter);
		}

		[Test]
		public void ClearPages_Should_Set_TempData_Message_And_Clear_Cache_And_Clear_All_Pages()
		{
			// Arrange		
			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			_pageCache.RemoveAll();
			_listCache.RemoveAll();
			_siteCache.RemoveAll();
			_pageCache.Add(1, new PageViewModel());
			_listCache.Add("list.somekey", new List<string>());
			_siteCache.AddMenu("should not be cleared");

			// Act
			RedirectToRouteResult result = _toolsController.ClearPages() as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
			
			Assert.That(_toolsController.TempData["SuccessMessage"], Is.EqualTo(SiteStrings.SiteSettings_Tools_ClearDatabase_Message));
			Assert.That(_cache.Count(), Is.EqualTo(1));
			Assert.That(_repository.AllPages().Count(), Is.EqualTo(0));
		}

		[Test]
		public void ExportAsSql_Should_Set_Filename_And_ContentType_And_FileStream_Should_Not_Be_Zero()
		{
			// Arrange
			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			FileStreamResult result = _toolsController.ExportAsSql() as FileStreamResult;

			// Assert
			Assert.That(result, Is.Not.Null, "FileStreamResult");
			Assert.That(result.FileDownloadName, Is.EqualTo("roadkill-export.sql"));
			Assert.That(result.ContentType, Is.EqualTo("text/plain"));
			Assert.That(result.FileStream.Length, Is.GreaterThan(0));
		}

		[Test]
		public void ExportAsWikiFiles_Should()
		{
			// Arrange
			string downloadFilename = string.Format("export-{0}.zip", DateTime.UtcNow.ToString("yyyy-MM-dd-HHmm"));
			string fullPath = Path.Combine(_wikiExporter.ExportFolder, downloadFilename);

			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			FilePathResult result = _toolsController.ExportAsWikiFiles() as FilePathResult;

			// Assert
			Assert.That(result, Is.Not.Null, "FileStreamResult");
			Assert.That(result.FileName, Is.EqualTo(fullPath));
			Assert.That(result.FileDownloadName, Is.EqualTo(downloadFilename));
			Assert.That(result.ContentType, Is.EqualTo("application/zip"));
		}

		[Test]
		public void ExportAsXml_Should_Set_Filename_And_ContentType_And_FileStream_Should_Not_Be_Zero()
		{
			// Arrange
			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			FileStreamResult result = _toolsController.ExportAsXml() as FileStreamResult;

			// Assert
			Assert.That(result, Is.Not.Null, "FileStreamResult");
			Assert.That(result.FileDownloadName, Is.EqualTo("roadkill-export.xml"));
			Assert.That(result.ContentType, Is.EqualTo("text/xml"));
			Assert.That(result.FileStream.Length, Is.GreaterThan(0));
		}

		[Test]
		public void ExportAttachments_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.ExportAttachments();

			// Assert
		}

		[Test]
		public void ImportFromScrewTurn_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.ImportFromScrewTurn("");

			// Assert
		}

		[Test]
		public void Index_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.Index();

			// Assert
		}

		[Test]
		public void RenameTag_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.RenameTag("", "");

			// Assert
		}

		[Test]
		public void UpdateSearchIndex_Should()
		{
			// Arrange

			// Act
			ActionResult result = _toolsController.UpdateSearchIndex();

			// Assert
		}
	}

	[TestFixture]
	[Category("Unit")]
	public class WikiExporterTests
	{
		private MocksAndStubsContainer _container;
		private RepositoryMock _repository;
		private PageService _pageService;
		private PluginFactoryMock _pluginFactory;
		private WikiExporter _wikiExporter;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_repository = _container.Repository;
			_pageService = _container.PageService;
			_pluginFactory = _container.PluginFactory;

			_wikiExporter = new WikiExporter(_pageService, _repository, _pluginFactory);
			_wikiExporter.ExportFolder = AppDomain.CurrentDomain.BaseDirectory;
		}

		[Test]
		public void ExportAsXml_Should_Return_Non_Empty_Stream()
		{
			// Arrange
			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			Stream stream = _wikiExporter.ExportAsXml();

			// Assert
			Assert.That(stream.Length, Is.GreaterThan(1));
		}

		[Test]
		public void ExportAsSql_Should_Return_Non_Empty_Stream()
		{
			// Arrange
			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			Stream stream = _wikiExporter.ExportAsSql();

			// Assert
			Assert.That(stream.Length, Is.GreaterThan(1));
		}

		[Test]
		public void ExportAsWikiFiles_Should_Save_File_To_Export_Directory()
		{
			// Arrange
			string filename = string.Format("export-{0}.zip", DateTime.Now.Ticks);
			string zipFullPath = Path.Combine(_wikiExporter.ExportFolder, filename);

			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			_wikiExporter.ExportAsWikiFiles(filename);

			// Assert
			Assert.That(File.Exists(zipFullPath), Is.True);

			FileInfo file = new FileInfo(zipFullPath);
			Assert.That(file.Length, Is.GreaterThan(1));
		}
	}
}
