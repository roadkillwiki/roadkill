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
using Roadkill.Core.Localization.Resx;
using Roadkill.Core.Managers;
using Roadkill.Core.Security;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class HelpControllerTests
	{
		private ApplicationSettings _settings;
		private IUserContext _context;
		private RepositoryMock _repository;

		private UserManagerBase _userManager;
		private SettingsManager _settingsManager;
		private HelpController _controller;

		[SetUp]
		public void Setup()
		{
			_context = new Mock<IUserContext>().Object;
			_settings = new ApplicationSettings();
			_settings.Installed = true;
			_userManager = new FormsAuthUserManager(_settings, _repository);

			_controller = new HelpController(_settings,  _userManager, _context, _settingsManager);
		}

		[Test]
		public void CreoleReference_Should_Return_View()
		{
			// Arrange


			// Act
			ViewResult result = _controller.CreoleReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void MediaWikiReference_Should_Return_View()
		{
			// Arrange


			// Act
			ViewResult result = _controller.MediaWikiReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}

		[Test]
		public void MarkdownReference_Should_Return_View()
		{
			// Arrange


			// Act
			ViewResult result = _controller.MarkdownReference() as ViewResult;

			// Assert
			Assert.That(result, Is.Not.Null);
		}
	}
}
