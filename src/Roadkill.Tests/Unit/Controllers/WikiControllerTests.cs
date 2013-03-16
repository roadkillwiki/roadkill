using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Controllers;
using Roadkill.Core.Converters;
using Roadkill.Core.Database;
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Search;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class WikiControllerTests
	{
		private IConfigurationContainer _config;
		private IRoadkillContext _context;
		private IRepository _repository;

		private UserManager _userManager;
		private PageManager _pageManager;
		private HistoryManager _historyManager;
		private List<Page> _pages;
		private List<PageContent> _pagesContent;

		[SetUp]
		public void Init()
		{
			_pages = new List<Page>();
			_pagesContent = new List<PageContent>();

			_context = new Mock<IRoadkillContext>().Object;
			_config = new RoadkillSettings();
			_config.ApplicationSettings = new ApplicationSettings();
			_config.ApplicationSettings.Installed = true;
			_config.SitePreferences = new SitePreferences() { AllowedFileTypes = "png, jpg" };
			_config.ApplicationSettings.AttachmentsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "attachments");

			// Dependencies for PageManager
			Mock<IRepository> repositoryMock = new Mock<IRepository>();
			repositoryMock.Setup(x => x.GetPageById(It.IsAny<int>())).Returns<int>(x => _pages.FirstOrDefault(p => p.Id == x));
			repositoryMock.Setup(x => x.GetLatestPageContent(It.IsAny<int>())).Returns<int>((id) => _pagesContent.FirstOrDefault(p => p.Page.Id == id));

			Mock<SearchManager> searchMock = new Mock<SearchManager>();

			_repository = repositoryMock.Object;
			_userManager = new Mock<UserManager>(_config, null).Object;
			_historyManager = new HistoryManager(_config, _repository, _context);
			_pageManager = new PageManager(_config, _repository, null, _historyManager, _context);
		}

		[Test]
		public void Index_Should_Return_Page()
		{
			// Arrange
			WikiController wikiController = new WikiController(_config, _userManager, _pageManager, _context);
			MvcMockContainer mocksContainer = wikiController.SetFakeControllerContext();
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
			_pages.Add(page1);
			_pagesContent.Add(page1Content);

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
			WikiController wikiController = new WikiController(_config, _userManager, _pageManager, _context);
			MvcMockContainer mocksContainer = wikiController.SetFakeControllerContext();

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
