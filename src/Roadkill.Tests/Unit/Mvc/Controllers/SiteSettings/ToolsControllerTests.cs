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
using Roadkill.Core.Domain.Export;
using Roadkill.Core.Import;
using Roadkill.Core.Localization;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

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
		private WikiImporterMock _wikiImporter;
		private PluginFactoryMock _pluginFactory;
		private SearchServiceMock _searchService;
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
			_container.ClearCache();

			_pageService = _container.PageService;
			_wikiImporter = new WikiImporterMock();
			_pluginFactory = _container.PluginFactory;
			_searchService = _container.SearchService;

			// There's no point mocking WikiExporter (and turning it into an interface) as it 
			// a lot of usefulness of these tests would be lost when creating fake Streams and zip files.
			_wikiExporter = new WikiExporter(_applicationSettings, _pageService, _repository, _pluginFactory);
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
		public void ExportAsWikiFiles_Should_Set_Filename_And_ContentType()
		{
			// Arrange
			string fullPath = Path.Combine(_wikiExporter.ExportFolder, "export-");

			_repository.AddNewPage(new Page() { Id = 1 }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2 }, "text", "admin", DateTime.UtcNow);

			// Act
			FilePathResult result = _toolsController.ExportAsWikiFiles() as FilePathResult;

			// Assert
			Assert.That(result, Is.Not.Null, "FileStreamResult");
			Assert.That(result.FileName, Is.StringStarting(fullPath));
			Assert.That(result.FileDownloadName, Is.StringStarting("export-"));
			Assert.That(result.FileDownloadName, Is.StringEnding(".zip"));
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
		public void ExportAttachments_Should_Set_Filename_And_ContentType()
		{
			// Arrange
			string fullPath = Path.Combine(_wikiExporter.ExportFolder, "attachments-");

			// Act
			FilePathResult result = _toolsController.ExportAttachments() as FilePathResult;

			// Assert
			Assert.That(result, Is.Not.Null, "FileStreamResult");
			Assert.That(result.FileName, Is.StringStarting(fullPath));
			Assert.That(result.FileDownloadName, Is.StringStarting("attachments-"));
			Assert.That(result.FileDownloadName, Is.StringEnding(".zip"));
			Assert.That(result.ContentType, Is.EqualTo("application/zip"));
		}

		[Test]
		public void ImportFromScrewTurn_Should_TempData_Error_Message_When_ConnectionString_Is_Empty()
		{
			// Arrange

			// Act
			RedirectToRouteResult result = _toolsController.ImportFromScrewTurn("") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(_toolsController.TempData["ErrorMessage"], Is.Not.Null.Or.Empty);
		}

		[Test]
		public void ImportFromScrewTurn_Should_Redirect_And_TempData_Message_And_Import()
		{
			// Arrange

			// Act
			RedirectToRouteResult result = _toolsController.ImportFromScrewTurn("connection string") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(_toolsController.TempData["SuccessMessage"], Is.EqualTo(SiteStrings.SiteSettings_Tools_ScrewTurnImport_Message));

			Assert.That(_wikiImporter.ImportedFromSql, Is.True);
			Assert.That(_wikiImporter.UpdatedSearch, Is.True);
		}

		[Test]
		public void Index_Should_Return_View()
		{
			// Arrange

			// Act
			ViewResult result = _toolsController.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ViewResult");
		}

		[Test]
		public void RenameTag_Should_Redirect_And_Set_TempData_Message_And_Rename_Tag()
		{
			// Arrange
			_repository.AddNewPage(new Page() { Id = 1, Tags = "old" }, "text", "admin", DateTime.UtcNow);
			_repository.AddNewPage(new Page() { Id = 2, Tags = "old" }, "text", "admin", DateTime.UtcNow);

			// Act
			RedirectToRouteResult result = _toolsController.RenameTag("old", "new") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(_toolsController.TempData["SuccessMessage"], Is.EqualTo(SiteStrings.SiteSettings_Tools_RenameTag_Message));

			Assert.That(_repository.GetPageById(1).Tags, Is.StringContaining("new"));
			Assert.That(_repository.GetPageById(2).Tags, Is.StringContaining("new"));
		}

		[Test]
		public void SiteSettings_Should_Return_ContentResult_With_Json()
		{
			// Arrange

			// Act
			ContentResult result = _toolsController.SiteSettings() as ContentResult;

			// Assert
			Assert.That(result, Is.Not.Null, "ContentResult");
			Assert.That(result.Content, Is.StringContaining("{")); // a very basic JSON-format string test
			Assert.That(result.Content, Is.StringContaining("}"));
		}

		[Test]
		public void UpdateSearchIndex_Should_Redirect_And_Set_TempData_Message_And_Create_Index()
		{
			// Arrange

			// Act
			RedirectToRouteResult result = _toolsController.UpdateSearchIndex() as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null, "RedirectToRouteResult");
			Assert.That(result.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(_toolsController.TempData["SuccessMessage"], Is.EqualTo(SiteStrings.SiteSettings_Tools_RebuildSearch_Message));

			Assert.That(_searchService.CreatedNewIndex, Is.True);
		}
	}
}
