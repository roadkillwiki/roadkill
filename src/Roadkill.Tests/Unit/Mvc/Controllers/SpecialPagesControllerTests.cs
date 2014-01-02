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
using System.IO;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SpecialPagesControllerTests
	{
		private MocksAndStubsContainer _container;
		
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private RepositoryMock _repository;
		private UserServiceMock _userService;
		private SettingsService _settingsService;
		private PluginFactoryMock _pluginFactory;

		private SpecialPagesController _specialPagesController;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_context = _container.UserContext;
			_repository = _container.Repository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;

			_specialPagesController = new SpecialPagesController(_applicationSettings, _userService, _context, _settingsService, _pluginFactory);
		}

		[Test]
		public void Index_Should_Call_Plugin_GetResult()
		{
			// Arrange
			_pluginFactory.SpecialPages.Add(new SpecialPageMock());

			// Act
			ContentResult result = _specialPagesController.Index("kay") as ContentResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void Index_Should_Throw_HttpException_When_Plugin_Does_Not_Exist()
		{
			// Arrange + Act + Assert
			HttpException httpException = Assert.Throws<HttpException>(() => _specialPagesController.Index("badID"));
			Assert.That(httpException.GetHttpCode(), Is.EqualTo(404));
		}
	}
}
