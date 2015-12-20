using System.Web;
using System.Web.Mvc;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Mvc.Controllers
{
	[TestFixture]
	[Category("Unit")]
	public class SpecialPagesControllerTests
	{
		private MocksAndStubsContainer _container;
		
		private ApplicationSettings _applicationSettings;
		private IUserContext _context;
		private PageRepositoryMock _pageRepository;
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
			_pageRepository = _container.PageRepository;
			_pluginFactory = _container.PluginFactory;
			_settingsService = _container.SettingsService;
			_userService = _container.UserService;

			_specialPagesController = new SpecialPagesController(_applicationSettings, _userService, _context, _settingsService, _pluginFactory);
		}

		[Test]
		public void index_should_call_plugin_getresult()
		{
			// Arrange
			_pluginFactory.SpecialPages.Add(new SpecialPageMock());

			// Act
			ContentResult result = _specialPagesController.Index("kay") as ContentResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void index_should_throw_httpexception_when_plugin_does_not_exist()
		{
			// Arrange + Act + Assert
			HttpException httpException = Assert.Throws<HttpException>(() => _specialPagesController.Index("badID"));
			Assert.That(httpException.GetHttpCode(), Is.EqualTo(404));
		}
	}
}
