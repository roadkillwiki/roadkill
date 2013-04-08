using System;
using System.Collections.Specialized;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit;
using NUnit.Framework;
using Roadkill.Core;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	public class RoutingTests
	{
		[Test]
		[TestCase("~/Wiki/1/some-page", "Wiki", "Index", "1")]
		[TestCase("~/", "Home", "Index", null)]
		public void Page_Urls(string url, string expectedController, string expectedAction, object expectedParamValue)
		{
			// Arrange
			if (expectedParamValue == null)
				expectedParamValue = UrlParameter.Optional;

			var mockContext = new StubHttpContextForRouting("", url);

			RouteCollection routes = new RouteCollection();
			RoadkillApplication.RegisterRoutes(routes);

			// Act
			RouteData routeData = routes.GetRouteData(mockContext);

			// Assert
			Assert.IsNotNull(routeData);
			Assert.That(routeData.Values["controller"].ToString(), Is.EqualTo(expectedController));
			Assert.That(routeData.Values["action"].ToString(), Is.EqualTo(expectedAction));
			Assert.That(routeData.Values["id"], Is.EqualTo(expectedParamValue));
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
