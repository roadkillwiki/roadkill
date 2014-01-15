using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Cache;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.Controllers;
using Roadkill.Core.Plugins.SpecialPages.BuiltIn;
using Roadkill.Core.Security;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Plugins
{
	[TestFixture]
	[Category("Unit")]
	public class RandomPageTests
	{
		private MocksAndStubsContainer _container;
		private SpecialPagesController _controller;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();
			_controller = null; // not used
		}

		[Test]
		public void Should_Redirect_To_Home_When_No_Pages_Exist()
		{
			// Arrange
			RandomPage randomPage = new RandomPage(new RandomMock());
			randomPage.PageService = _container.PageService;

			// Act
			RedirectToRouteResult redirectResult = randomPage.GetResult(_controller) as RedirectToRouteResult;

			// Assert
			Assert.That(redirectResult, Is.Not.Null);
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Home"));
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
		}

		[Test]
		public void Should_Redirect_To_Correct_Controller_Using_Random()
		{
			// Arrange
			_container.Repository.AddNewPage(new Page() { Id = 1, Title = "1" }, "text", "", DateTime.Now);
			_container.Repository.AddNewPage(new Page() { Id = 2, Title = "2" }, "text", "", DateTime.Now);
			_container.Repository.AddNewPage(new Page() { Id = 3, Title = "3" }, "text", "", DateTime.Now);
			_container.Repository.AddNewPage(new Page() { Id = 4, Title = "4" }, "text", "", DateTime.Now);
			_container.Repository.AddNewPage(new Page() { Id = 5, Title = "5" }, "text", "", DateTime.Now);


			RandomPage randomPage = new RandomPage(new RandomMock());
			randomPage.PageService = _container.PageService;

			// Act
			RedirectToRouteResult redirectResult = randomPage.GetResult(_controller) as RedirectToRouteResult;

			// Assert
			Assert.That(redirectResult, Is.Not.Null);
			Assert.That(redirectResult.RouteValues["controller"], Is.EqualTo("Wiki"));
			Assert.That(redirectResult.RouteValues["action"], Is.EqualTo("Index"));
			Assert.That(redirectResult.RouteValues["id"], Is.EqualTo(4)); // 4 as it's zero based
		}

		public class RandomMock : Random
		{
			public override int Next(int minValue, int maxValue)
			{
				return 3;
			}
		}
	}
}
