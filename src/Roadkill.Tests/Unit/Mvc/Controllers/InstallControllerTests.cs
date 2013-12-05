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
using System.Threading;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class InstallControllerTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private PageService _pageService;
		private PageHistoryService _historyService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;
		private SearchServiceMock _searchService;

		private InstallController _installController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.Installed = false;

			_context = _container.UserContext;
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;
			_searchService = _container.SearchService;

			_installController = new InstallController(_applicationSettings, _userService, _pageService, _searchService, _repository, _settingsService, _context);
		}

		[Test]
		public void Index__Should_Return_ViewResult_And_Model_With_LanguageModels_And_Set_UILanguage_To_English()
		{
			// Arrange

			// Act
			ViewResult result = _installController.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			IEnumerable<LanguageViewModel> models = result.ModelFromActionResult<IEnumerable<LanguageViewModel>>();
			Assert.NotNull(models, "Null model");
			Assert.That(models.Count(), Is.GreaterThanOrEqualTo(1));
			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo("en"));
		}

		[Test]
		public void Step1_Should_Return_ViewResult_With_Language_Summary_And_Set_UICulture_From_Language()
		{
			// Arrange

			// Act
			ViewResult result = _installController.Step1("hi") as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);

			LanguageViewModel model = result.ModelFromActionResult<LanguageViewModel>();
			Assert.NotNull(model, "Null model");
			Assert.That(model.Code, Is.EqualTo("hi"));

			Assert.That(Thread.CurrentThread.CurrentUICulture.Name, Is.EqualTo("hi"));
		}
	}
}