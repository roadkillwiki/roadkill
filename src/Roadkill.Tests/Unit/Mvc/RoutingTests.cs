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
			RoadkillApplication.RegisterRoutes(RouteTable.Routes);
		}

		[Test]
		public void HomeController_Routes_Are_Mapped()
		{
			"~/".ShouldMapTo<HomeController>(action => action.Index());
			"~/home/globaljsvars".ShouldMapTo<HomeController>(action => action.GlobalJsVars());
			// Search isn't supported as it uses 'q' for its id parameter
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
		public void FilesController_Folder_Is_Registered()
		{
			"~/files/folder/some+great+folder".ShouldMapTo<FileManagerController>(action => action.FolderInfo("some+great+folder"));
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
			RoadkillApplication.RegisterRoutes(routes);		

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
			RoadkillApplication.RegisterRoutes(routes);

			// Act
			RouteData routeData = routes.GetRouteData(mockContext);

			// Assert
			Assert.IsNotNull(routeData);
			Assert.That(routeData.RouteHandler, Is.Not.TypeOf<AttachmentRouteHandler>());
		}

		[Test]
		public void Attachments_With_Custom_Route_Should_Have_Correct_Handler_And_Contain_Route_Values()
		{
			// Arrange
			ApplicationSettings settings = new ApplicationSettings();
			settings.AttachmentsRoutePath = "mywiki/Attachments";
			string filename = "somefile.png";
			string url = string.Format("~/{0}/{1}", settings.AttachmentsRoutePath, filename);
			var mockContext = new StubHttpContextForRouting("", url);

			RouteTable.Routes.Clear();
			RouteCollection routes = new RouteCollection();
			AttachmentRouteHandler.RegisterRoute(settings, routes);
			RoadkillApplication.RegisterRoutes(routes);

			// Act
			RouteData routeData = routes.GetRouteData(mockContext);

			// Assert
			Assert.IsNotNull(routeData);
			Assert.That(routeData.RouteHandler, Is.TypeOf<AttachmentRouteHandler>());
			Assert.That(routeData.Values["filename"].ToString(), Is.EqualTo(filename));
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
			RoadkillApplication.RegisterRoutes(routes);
			AttachmentRouteHandler.RegisterRoute(settings, routes);

			// Assert
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
