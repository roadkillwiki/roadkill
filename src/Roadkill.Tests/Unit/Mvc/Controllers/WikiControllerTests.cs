using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Localization;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;
using System.Runtime.Caching;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class WikiControllerTests
	{
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;

		private UserManagerBase _userManager;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;

		[SetUp]
		public void Setup()
		{
			_context = new Mock<IUserContext>().Object;
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.Installed = true;
			_applicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");

			_repository = new RepositoryMock();
			_repository.SiteSettings = new SiteSettings();
			_repository.SiteSettings.MarkupType = "Creole";

			// Cache
			ListCache listCache = new ListCache(_applicationSettings, MemoryCache.Default);
			PageSummaryCache pageSummaryCache = new PageSummaryCache(_applicationSettings, MemoryCache.Default);
			SiteCache siteCache = new SiteCache(_applicationSettings, MemoryCache.Default);

			// Dependencies for PageService
			_pluginFactory = new PluginFactoryMock();
			Mock<SearchService> searchMock = new Mock<SearchService>();

			_settingsService = new SettingsService(_applicationSettings, _repository);
			_userManager = new Mock<UserManagerBase>(_applicationSettings, null).Object;
			_historyService = new PageHistoryService(_applicationSettings, _repository, _context, pageSummaryCache, _pluginFactory);
			_pageService = new PageService(_applicationSettings, _repository, null, _historyService, _context, listCache, pageSummaryCache, siteCache, _pluginFactory);
		}

		[Test]
		public void Index_Should_Return_Page()
		{
			// Arrange
			WikiController wikiController = new WikiController(_applicationSettings, _userManager, _pageService, _context, _settingsService);
			wikiController.SetFakeControllerContext();
			Page page1 = new Page()
			{
				Id = 50,
				Tags = "homepage, tag2",
				Title = "Welcome to the site"
			};
			PageContent page1Content = new PageContent()
			{
				Id = Guid.NewGuid(),
				Page = page1,
				Text = "Hello world"
			};
			_repository.Pages.Add(page1);
			_repository.PageContents.Add(page1Content);

			// Act
			ActionResult result = wikiController.Index(50, "");

			// Assert
			Assert.That(result, Is.TypeOf<ViewResult>(), "ViewResult");

			PageSummary summary = result.ModelFromActionResult<PageSummary>();
			Assert.NotNull(summary, "Null model");
			Assert.That(summary.Title, Is.EqualTo(page1.Title));
			Assert.That(summary.Content, Is.EqualTo(page1Content.Text));
		}

		[Test]
		public void Index_With_Bad_Page_Id_Should_Redirect()
		{
			// Arrange
			WikiController wikiController = new WikiController(_applicationSettings, _userManager, _pageService, _context, _settingsService);
			wikiController.SetFakeControllerContext();

			// Act
			ActionResult result = wikiController.Index(0, "");

			// Assert
			Assert.That(result, Is.TypeOf<RedirectToRouteResult>(), "ViewResult");

			RedirectToRouteResult redirectResult = result as RedirectToRouteResult;
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
		}
	}
}
