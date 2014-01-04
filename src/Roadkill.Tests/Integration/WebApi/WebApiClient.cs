using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using RestSharp;
using Roadkill.Core.Mvc.Controllers.Api;

namespace Roadkill.Tests.Integration.WebApi
{
	public class WebApiClient
	{
		public string BaseUrl { get; set; }
		public RestClient Client { get; set; }

		public WebApiClient()
		{
			string url = ConfigurationManager.AppSettings["url"];
			if (string.IsNullOrEmpty(url))
				url = "http://localhost:9876";

			BaseUrl = url;

			Client = new RestClient(BaseUrl);
			Client.CookieContainer = new CookieContainer();
		}

		public void Login()
		{
			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = Settings.ADMIN_EMAIL,
				Password = Settings.ADMIN_PASSWORD
			};

			string url = GetResourcePath("Authenticate");
			RestRequest request = new RestRequest(url, Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(info);
			IRestResponse response = Client.ExecuteAsPost<UserController.UserInfo>(request, "POST");

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Login failed, status code wasn't 200: {0} {1} ", response.StatusCode, response.Content);
			Assert.That(response.Content, Is.EqualTo("true"), "Login failed, true wasn't returned: : {0}", response.Content);
		}

		public string GetResourcePath(string fullPath)
		{
			return string.Format("/api/{0}", fullPath);
		}

		public WebApiResponse Get(string url, Dictionary<string,string> arguments)
		{
			RestRequest request = new RestRequest(url, Method.GET);
			request.RequestFormat = DataFormat.Json;

			foreach (string key in arguments.Keys)
			{
				request.Parameters.Add(new Parameter() { Name = key, Value = arguments[key] });
			}
			IRestResponse response = Client.ExecuteAsGet(request, "GET");

			return new WebApiResponse() { Content = response.Content, HttpStatusCode = response.StatusCode };
		}

		public WebApiResponse Post<T>(string url, T jsonBody) where T: new()
		{
			RestRequest request = new RestRequest(url, Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(jsonBody);
			IRestResponse response = Client.ExecuteAsPost<T>(request, "POST");

			return new WebApiResponse() { Content = response.Content, HttpStatusCode = response.StatusCode };
		}

		public WebApiResponse Put<T>(string url, T jsonBody) where T : new()
		{
			RestRequest request = new RestRequest(url, Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(jsonBody);
			IRestResponse response = Client.ExecuteAsPost<T>(request, "PUT");

			return new WebApiResponse() { Content = response.Content, HttpStatusCode = response.StatusCode };
		}
	}

	public class WebApiResponse
	{
		public string Content { get; set; }
		public HttpStatusCode HttpStatusCode { get; set; }
	}
}
