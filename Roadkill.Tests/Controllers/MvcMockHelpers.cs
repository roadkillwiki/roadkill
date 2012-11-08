using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Moq;

namespace Roadkill.Tests.Controllers
{
	/// <summary>
	/// Based on http://www.hanselman.com/blog/ASPNETMVCSessionAtMix08TDDAndMvcMockHelpers.aspx
	/// </summary>
	public static class MvcMockHelpers
	{
		public static HttpContextBase FakeHttpContext(MvcMockContainer container)
		{
			var context = new Mock<HttpContextBase>();
			var request = new Mock<HttpRequestBase>();
			var response = new Mock<HttpResponseBase>();
			var session = new Mock<HttpSessionStateBase>();
			var server = new Mock<HttpServerUtilityBase>();

			request.Setup(r => r.HttpMethod).Returns("POST");
			request.Setup(r => r.Headers).Returns(new NameValueCollection());
			request.Setup(r => r.Form).Returns(new NameValueCollection());
			request.Setup(r => r.QueryString).Returns(new NameValueCollection());

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

		public static MvcMockContainer SetFakeControllerContext(this Controller controller)
		{
			MvcMockContainer container = new MvcMockContainer();
			var httpContext = FakeHttpContext(container);
			ControllerContext context = new ControllerContext(new RequestContext(httpContext, new RouteData()), controller);
			controller.ControllerContext = context;

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

			mock.Setup(req => req.QueryString)
				.Returns(GetQueryStringParameters(url));
			mock.Setup(req => req.AppRelativeCurrentExecutionFilePath)
				.Returns(GetUrlFileName(url));
			mock.Setup(req => req.PathInfo)
				.Returns(string.Empty);
		}

		public static T ModelFromActionResult<T>(this ActionResult actionResult)
		{
			// Taken from Stackoverflow
			object model;
			if (actionResult.GetType() == typeof(ViewResult))
			{
				ViewResult viewResult = (ViewResult)actionResult;
				model = viewResult.Model;
			}
			else if (actionResult.GetType() == typeof(PartialViewResult))
			{
				PartialViewResult partialViewResult = (PartialViewResult)actionResult;
				model = partialViewResult.Model;
			}
			else
			{
				throw new InvalidOperationException(string.Format("Actionresult of type {0} is not supported by ModelFromResult extractor.", actionResult.GetType()));
			}

			T typedModel = (T)model;
			return typedModel;
		}
	}
}
