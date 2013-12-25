using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Mvc.Controllers;
using MvcContrib.TestHelper;
using Roadkill.Core.Attachments;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class RoutingTests
	{
		[SetUp]
		public void Setup()
		{
			RouteTable.Routes.Clear();
			AttachmentRouteHandler.RegisterRoute(new ApplicationSettings(), RouteTable.Routes);
			Routing.Register(RouteTable.Routes);
		}

		[Test]
		public void HomeController_Routes_Are_Mapped()
		{
			"~/".ShouldMapTo<HomeController>(action => action.Index());

			RouteData routeData = "~/home/globaljsvars".WithMethod(HttpVerbs.Get);
			routeData.Values["version"] = "xyz";
			routeData.ShouldMapTo<HomeController>(action => action.GlobalJsVars("xyz"));

			routeData = "~/home/search".WithMethod(HttpVerbs.Get);
			routeData.Values["q"] = "searchquery";
			routeData.ShouldMapTo<HomeController>(action => action.Search("searchquery"));
		}

		[Test]
		public void WikiController_Maps_Id_And_Title()
		{
			"~/wiki/42".ShouldMapTo<WikiController>(action => action.Index(42,""));
			"~/wiki/42/my-page-name".ShouldMapTo<WikiController>(action => action.Index(42, "my-page-name"));
		}

		[Test]
		public void PagesController_ByUser_Maps_Values()
		{
			"~/Pages/byuser/ZWRpdG9yICg5Ni4yNTIuMTQwLjE3OSk%3d".ShouldMapTo<PagesController>(action => action.ByUser("ZWRpdG9yICg5Ni4yNTIuMTQwLjE3OSk%3d", null));
			"~/Pages/byuser/ZWRpdG9yICg5Ni4yNTIuMTQwLjE3OSk%3d/True".ShouldMapTo<PagesController>(action => action.ByUser("ZWRpdG9yICg5Ni4yNTIuMTQwLjE3OSk%3d", true));
		}

		[Test]
		public void FileManagerController_Should_Have_Index()
		{
			"~/filemanager".ShouldMapTo<FileManagerController>(action => action.Index());
		}

		[Test]
		public void Special_Route_Should_Map_To_SpecialPagesController()
		{
			"~/wiki/Special:Random".ShouldMapTo<SpecialPagesController>(action => action.Index("Random"));
		}

		[Test]
		public void HelpCheatsheet_Route_Should_Map_To_HelpController()
		{
			"~/wiki/Help:Cheatsheet".ShouldMapTo<HelpController>(action => action.Index());
		}

		[Test]
		public void HelpAbout_Route_Should_Map_To_HelpController()
		{
			"~/wiki/Help:About".ShouldMapTo<HelpController>(action => action.About());
		}

		[Test]
		public void Attachments_Should_Have_Correct_Handler_And_Contain_Route_Values()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			string filename = "somefile.png";
			string url = string.Format("~/{0}/{1}", settings.AttachmentsRoutePath, filename);
			var mockContext = new StubHttpContextForRouting("", url);

			RouteTable.Routes.Clear();
			RouteCollection routes = new RouteCollection();
			AttachmentRouteHandler.RegisterRoute(settings, routes); // has to be registered first
			Routing.Register(routes);

			// Act
			RouteData routeData = routes.GetRouteData(mockContext);

			// Assert
			Assert.IsNotNull(routeData);
			Assert.That(routeData.RouteHandler, Is.TypeOf<AttachmentRouteHandler>());
			Assert.That(routeData.Values["filename"].ToString(), Is.EqualTo(filename));
		}

		[Test]
		public void Attachments_In_Standard_Controller_Path_Should_Not_Map_To_Attachments_Handler()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			string url = "/pages/6/attachments-are-us";
			var mockContext = new StubHttpContextForRouting("", url);

			RouteTable.Routes.Clear();
			RouteCollection routes = new RouteCollection();
			AttachmentRouteHandler.RegisterRoute(settings, routes);
			Routing.Register(routes);

			// Act
			RouteData routeData = routes.GetRouteData(mockContext);

			// Assert
			Assert.IsNotNull(routeData);
			Assert.That(routeData.RouteHandler, Is.Not.TypeOf<AttachmentRouteHandler>());
		}

		[Test]
		[ExpectedException(typeof(ConfigurationException))]
		public void AttachmentsRoute_Using_Files_Route_Should_Throw_Exception()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.AttachmentsRoutePath = "Files";
			string filename = "somefile.png";
			string url = string.Format("~/{0}/{1}", settings.AttachmentsRoutePath, filename);
			var mockContext = new StubHttpContextForRouting("", url);

			RouteTable.Routes.Clear();
			RouteCollection routes = new RouteCollection();

			// Act
			Routing.Register(RouteTable.Routes);
			AttachmentRouteHandler.RegisterRoute(settings, routes);

			// Assert
		}

		[Test]
		[Description("Needs some work to get it to check it works without the virtual path modifier")]
		public void Attachments_With_SubApplication_Should_Have_Correct_Handler_And_Contain_Route_Values()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.AttachmentsRoutePath = "Attachments";
			string filename = "somefile.png";
			string url = string.Format("~/{0}/{1}", settings.AttachmentsRoutePath, filename); // doesn't work without the ~
			var mockContext = new StubHttpContextForRouting("/mywiki/", url);

			RouteTable.Routes.Clear();
			RouteCollection routes = new RouteCollection();
			AttachmentRouteHandler.RegisterRoute(settings, routes);

			// Act
			RouteData routeData = routes.GetRouteData(mockContext);

			// Assert
			Assert.IsNotNull(routeData);
			Assert.That(routeData.RouteHandler, Is.TypeOf<AttachmentRouteHandler>());
			Assert.That(routeData.Values["filename"].ToString(), Is.EqualTo(filename));
		}
	}

	public class StubHttpContextForRouting : HttpContextBase
	{
		private StubHttpRequestForRouting _request;
		private StubHttpResponseForRouting _response;

		public StubHttpContextForRouting(string appPath = "/", string requestUrl = "~/")
		{
			_request = new StubHttpRequestForRouting(appPath, requestUrl);
			_response = new StubHttpResponseForRouting();
		}

		public override HttpRequestBase Request
		{
			get { return _request; }
		}

		public override HttpResponseBase Response
		{
			get { return _response; }
		}
	}

	public class StubHttpRequestForRouting : HttpRequestBase
	{
		private string _appPath;
		private string _requestUrl;

		public StubHttpRequestForRouting(string appPath, string requestUrl)
		{
			_appPath = appPath;
			_requestUrl = requestUrl;
		}

		public override string AppRelativeCurrentExecutionFilePath
		{
			get { return _requestUrl; }
		}

		public override string ApplicationPath
		{
			get { return _appPath; }
		}

		public override string PathInfo
		{
			get { return ""; }
		}

		public override NameValueCollection ServerVariables
		{
			get { return new NameValueCollection(); }
		}
	}

	public class StubHttpResponseForRouting : HttpResponseBase
	{
		public override string ApplyAppPathModifier(string virtualPath)
		{
			return virtualPath;
		}
	}
}
