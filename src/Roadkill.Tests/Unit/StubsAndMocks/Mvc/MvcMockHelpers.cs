using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;
using NUnit.Framework;
using Roadkill.Core.Mvc;
using Roadkill.Tests.Unit.StubsAndMocks.Mvc;

namespace Roadkill.Tests.Unit
{
	/// <summary>
	/// Based on http://www.hanselman.com/blog/ASPNETMVCSessionAtMix08TDDAndMvcMockHelpers.aspx
	/// </summary>
	public static class MvcMockHelpers
	{
		public static HttpContextBase FakeHttpContext(MvcMockContainer container)
		{
			var context = new Mock<HttpContextBase>();
			var cache = new Mock<HttpCachePolicyBase>();
			var request = new Mock<HttpRequestBase>();
			var response = new Mock<HttpResponseBase>();
			var session = new Mock<HttpSessionStateBase>();
			var server = new Mock<HttpServerUtilityBase>();

			request.Setup(r => r.HttpMethod).Returns("POST");
			request.Setup(r => r.Headers).Returns(new NameValueCollection());
			request.Setup(r => r.Form).Returns(new NameValueCollection());
			request.Setup(r => r.QueryString).Returns(new NameValueCollection());

			response.Setup(r => r.Cache).Returns(new HttpCachePolicyMock());
			response.SetupProperty(r => r.StatusCode);
			response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>(x => { return x; }); // UrlHelper support

			server.Setup(s => s.UrlDecode(It.IsAny<string>())).Returns<string>(s => s);
			server.Setup(s => s.MapPath(It.IsAny<string>())).Returns<string>(s => { return s.Replace("~/bin", AppDomain.CurrentDomain.BaseDirectory +@"\").Replace("/",@"\"); });

			context.Setup(ctx => ctx.Request).Returns(request.Object);
			context.Setup(ctx => ctx.Response).Returns(response.Object);
			context.Setup(ctx => ctx.Session).Returns(session.Object);
			context.Setup(ctx => ctx.Server).Returns(server.Object);

			container.Context = context;
			container.Request = request;
			container.Response = response;
			container.SessionState = session;
			container.ServerUtility = server;

			return context.Object;
		}

		public static HttpContextBase FakeHttpContext(string url)
		{
			MvcMockContainer container = new MvcMockContainer();
			HttpContextBase context = FakeHttpContext(container);
			context.Request.SetupRequestUrl(url);
			return context;
		}

		public static MvcMockContainer SetFakeControllerContext(this Controller controller, string url = "")
		{
			MvcMockContainer container = new MvcMockContainer();
			var httpContext = FakeHttpContext(container);

			if (!string.IsNullOrEmpty(url))
				httpContext.Request.SetupRequestUrl(url);

			// Routes		
			RouteTable.Routes.Clear();
			Routing.Register(RouteTable.Routes);
			var routeData = new RouteData();
			routeData.Values["controller"] = "wiki";
			routeData.Values["action"] = "index";

			ControllerContext context = new ControllerContext(new RequestContext(httpContext, routeData), controller);
			controller.ControllerContext = context;
			controller.Url = new UrlHelper(new RequestContext(httpContext, routeData), RouteTable.Routes);

			return container;
		}

		static string GetUrlFileName(string url)
		{
			if (url.Contains("?"))
				return url.Substring(0, url.IndexOf("?"));
			else
				return url;
		}

		static NameValueCollection GetQueryStringParameters(string url)
		{
			if (url.Contains("?"))
			{
				NameValueCollection parameters = new NameValueCollection();

				string[] parts = url.Split("?".ToCharArray());
				string[] keys = parts[1].Split("&".ToCharArray());

				foreach (string key in keys)
				{
					string[] part = key.Split("=".ToCharArray());
					parameters.Add(part[0], part[1]);
				}

				return parameters;
			}
			else
			{
				return null;
			}
		}

		public static void SetHttpMethodResult(this HttpRequestBase request, string httpMethod)
		{
			Mock.Get(request)
				.Setup(req => req.HttpMethod)
				.Returns(httpMethod);
		}

		public static void SetupRequestUrl(this HttpRequestBase request, string url)
		{
			if (url == null)
				throw new ArgumentNullException("url");

			if (!url.StartsWith("~/"))
				throw new ArgumentException("Sorry, we expect a virtual url starting with \"~/\".");

			var mock = Mock.Get(request);
			mock.Setup(req => req.QueryString).Returns(GetQueryStringParameters(url));
			mock.Setup(req => req.AppRelativeCurrentExecutionFilePath).Returns(GetUrlFileName(url));
			mock.Setup(req => req.PathInfo).Returns(string.Empty);
			mock.Setup(req => req.Path).Returns(url);
			mock.Setup(req => req.ApplicationPath).Returns("/"); // essential for UrlHelper
		}
	}
}
