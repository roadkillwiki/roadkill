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
using Roadkill.Tests.Unit.StubsAndMocks;
using System;
using System.Web;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SpecialPagesControllerTests
	{
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceBase _userService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;

		private SpecialPagesController _controller;

		[SetUp]
		public void Setup()
		{
			_context = new Mock<IUserContext>().Object;
			_applicationSettings = new ApplicationSettings();
			_applicationSettings.Installed = true;
			_repository = new RepositoryMock();

			// Cache
			ListCache listCache = new ListCache(_applicationSettings, CacheMock.RoadkillCache);
			PageViewModelCache pageViewModelCache = new PageViewModelCache(_applicationSettings, CacheMock.RoadkillCache);
			SiteCache siteCache = new SiteCache(_applicationSettings, CacheMock.RoadkillCache);

			_pluginFactory = new PluginFactoryMock();
			Mock<SearchService> searchMock = new Mock<SearchService>();

			_settingsService = new SettingsService(_applicationSettings, _repository);
			_userService = new Mock<UserServiceBase>(_applicationSettings, null).Object;

			_controller = new SpecialPagesController(_applicationSettings, _userService, _context, _settingsService, _pluginFactory);
		}

		[Test]
		public void Index_Should_Call_Plugin_GetResult()
		{
			// Arrange
			_pluginFactory.SpecialPages.Add(new SpecialPageMock());

			// Act
			ContentResult result = _controller.Index("kay") as ContentResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Index_Should_Throw_HttpException_When_Plugin_Does_Not_Exist()
		{
			// Arrange + Act + Assert
			HttpException httpException = Assert.Throws<HttpException>(() => _controller.Index("badID"));
			Assert.That(httpException.GetHttpCode(), Is.EqualTo(404));
		}
	}
}
