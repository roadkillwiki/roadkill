using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.Mvc.Controllers;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PluginSettingsControllerTests
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
		private ListCache _listCache;
		private SiteCache _siteCache;
		private PageViewModelCache _pageViewModelCache;
		private MemoryCache _memoryCache;

		private PluginSettingsController _controller;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer(true);

			_applicationSettings = _container.ApplicationSettings;
			_applicationSettings.UseObjectCache = true;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;
			_historyService = _container.HistoryService;
			_pageService = _container.PageService;

			_listCache = _container.ListCache;
			_siteCache = _container.SiteCache;
			_pageViewModelCache = _container.PageViewModelCache;
			_memoryCache = _container.MemoryCache;

			_controller = new PluginSettingsController(_applicationSettings, _userService, _context, _settingsService, _pluginFactory, _repository, _siteCache, _pageViewModelCache, _listCache);
		}

		[Test]
		public void Index_Should_Return_ViewResult_And_Model_With_2_PluginModels_Ordered_By_Name()
		{
			// Arrange
			TextPluginStub pluginB = new TextPluginStub("b id", "b name", "b desc");
			pluginB.Repository = _repository;
			pluginB.PluginCache = new SiteCache(new ApplicationSettings(), CacheMock.RoadkillCache);

			TextPluginStub pluginA = new TextPluginStub("a id", "a name", "a desc");
			pluginA.Repository = _repository;
			pluginA.PluginCache = _siteCache;

			_pluginFactory.RegisterTextPlugin(pluginB); // reverse the order to test the ordering
			_pluginFactory.RegisterTextPlugin(pluginA);

			// Act
			ViewResult result = _controller.Index() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			IEnumerable<PluginViewModel> pluginModels = result.ModelFromActionResult<IEnumerable<PluginViewModel>>();
			Assert.NotNull(pluginModels, "Null model");

			List<PluginViewModel> pageModelList = pluginModels.ToList();

			Assert.That(pageModelList.Count(), Is.EqualTo(2));
			Assert.That(pageModelList[0].Name, Is.EqualTo("a name"));
			Assert.That(pageModelList[1].Name, Is.EqualTo("b name"));
		}

		[Test]
		public void Edit_GET_Should_Return_ViewResult_And_Model_With_Known_Values()
		{
			// Arrange		
			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = _repository;
			plugin.PluginCache = _siteCache;
			
			_repository.SaveTextPluginSettings(plugin);
			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			ViewResult result = _controller.Edit(plugin.Id) as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
			PluginViewModel model = result.ModelFromActionResult<PluginViewModel>();
			Assert.NotNull(model, "Null model");

			Assert.That(model.Id, Is.EqualTo(plugin.Id));
			Assert.That(model.Name, Is.EqualTo(plugin.Name));
			Assert.That(model.Description, Is.EqualTo(plugin.Description));
		}

		[Test]
		public void Edit_GET_Should_Load_Settings_From_Repository()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = _repository;
			plugin.PluginCache = _siteCache;
			plugin.Settings.SetValue("name1", "default-value1");
			plugin.Settings.SetValue("name2", "default-value2");

			RepositoryMock repositoryMock = new RepositoryMock();
			repositoryMock.SaveTextPluginSettings(plugin);
			repositoryMock.TextPlugins[0].Settings.SetValue("name1", "value1");
			repositoryMock.TextPlugins[0].Settings.SetValue("name2", "value2");

			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			ViewResult result = _controller.Edit(plugin.Id) as ViewResult;

			// Assert
			PluginViewModel model = result.ModelFromActionResult<PluginViewModel>();
			Assert.That(model.SettingValues[0].Value, Is.EqualTo("value1"));
			Assert.That(model.SettingValues[1].Value, Is.EqualTo("value2"));
		}

		[Test]
		public void Edit_GET_Should_Use_Default_Plugin_Settings_When_Plugin_Doesnt_Exist_In_Repository()
		{
			// Arrange
			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = _repository;
			plugin.PluginCache = _siteCache;
			plugin.Settings.SetValue("name1", "default-value1");
			plugin.Settings.SetValue("name2", "default-value2");

			_pluginFactory.RegisterTextPlugin(plugin);

			// Act
			ViewResult result = _controller.Edit(plugin.Id) as ViewResult;

			// Assert
			PluginViewModel model = result.ModelFromActionResult<PluginViewModel>();
			Assert.That(model.SettingValues[0].Value, Is.EqualTo("default-value1"));
			Assert.That(model.SettingValues[1].Value, Is.EqualTo("default-value2"));
		}

		[Test]
		public void Edit_GET_Should_Redirect_When_Id_Is_Empty()
		{
			// Arrange

			// Act
			RedirectToRouteResult result = _controller.Edit("") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Edit_GET_Should_Redirect_When_Plugin_Does_Not_Exist()
		{
			// Arrange

			// Act
			RedirectToRouteResult result = _controller.Edit("somepluginId") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Edit_POST_Should_Save_Setting_Values_To_Repository_From_Model_And_Clear_SiteCache()
		{
			// Arrange
			_pageViewModelCache.Add(1, new PageViewModel()); // dummmy items
			_listCache.Add("a key", new List<string>() { "1", "2" });

			TextPluginStub plugin = new TextPluginStub();
			plugin.Repository = _repository;
			plugin.PluginCache = _siteCache;
			plugin.Settings.SetValue("name1", "default-value1");
			plugin.Settings.SetValue("name2", "default-value2");

			_pluginFactory.RegisterTextPlugin(plugin);

			PluginViewModel model = new PluginViewModel();
			model.Id = plugin.Id;
			model.SettingValues = new List<SettingValue>();
			model.SettingValues.Add(new SettingValue() { Name = "name1", Value = "new-value1" });
			model.SettingValues.Add(new SettingValue() { Name = "name2", Value = "new-value2" });

			// Act
			ViewResult result = _controller.Edit(model) as ViewResult;

			// Assert
			List<SettingValue> values = _repository.TextPlugins[0].Settings.Values.ToList();
			Assert.That(values[0].Value, Is.EqualTo("new-value1"));
			Assert.That(values[1].Value, Is.EqualTo("new-value2"));

			Assert.That(_memoryCache.Count(), Is.EqualTo(0));
		}

		[Test]
		public void Edit_POST_Should_Redirect_When_Plugin_Does_Not_Exist()
		{
			// Arrange

			// Act
			RedirectToRouteResult result = _controller.Edit("somepluginId") as RedirectToRouteResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}
	}
}
