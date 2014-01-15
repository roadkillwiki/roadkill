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
			//Client = new RestClient("http://roadkill.local");
			Client.CookieContainer = new CookieContainer();
		}

		/// <summary>
		/// Calls the Authenticate() web api method for further get/post/put calls.
		/// </summary>
		public void Login()
		{
			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = Settings.ADMIN_EMAIL,
				Password = Settings.ADMIN_PASSWORD
			};

			string url = GetFullPath("Authenticate");
			RestRequest request = new RestRequest(url, Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(info);
			IRestResponse response = Client.ExecuteAsPost<UserController.UserInfo>(request, "POST");

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Login failed, status code wasn't 200: {0} {1} ", response.StatusCode, response.Content);
			Assert.That(response.Content, Is.EqualTo("true"), "Login failed, true wasn't returned: : {0}", response.Content);
		}

		public string GetFullPath(string fullPath)
		{
			return string.Format("/api/{0}", fullPath);
		}

		public WebApiResponse Get(string url, Dictionary<string, string> queryString = null)
		{
			url = GetFullPath(url);
			RestRequest request = new RestRequest(url, Method.GET);
			request.RequestFormat = DataFormat.Json;

			if (queryString != null)
			{
				foreach (string key in queryString.Keys)
				{
					Parameter parameter = new Parameter()
					{
						Name = key,
						Value = queryString[key],
						Type = ParameterType.QueryString
					};

					request.Parameters.Add(parameter);
				}
			}

			IRestResponse response = Client.ExecuteAsGet(request, "GET");
			return new WebApiResponse()
			{
				Url = Client.BuildUri(request).ToString(), 
				Content = response.Content, 
				HttpStatusCode = response.StatusCode 
			};
		}

		public WebApiResponse<TResult> Get<TResult>(string url, Dictionary<string, string> queryString = null) where TResult : new()
		{
			url = GetFullPath(url);
			RestRequest request = new RestRequest(url, Method.GET);
			request.RequestFormat = DataFormat.Json;

			if (queryString != null)
			{
				foreach (string key in queryString.Keys)
				{
					Parameter parameter = new Parameter()
					{
						Name = key,
						Value = queryString[key],
						Type = ParameterType.QueryString
					};

					request.Parameters.Add(parameter);
				}
			}

			IRestResponse<TResult> response = Client.ExecuteAsGet<TResult>(request, "GET");
			return new WebApiResponse<TResult>()
			{
				Result = response.Data,
				Url = Client.BuildUri(request).ToString(),
				Content = response.Content,
				HttpStatusCode =
				response.StatusCode
			};
		}

		public WebApiResponse Post<T>(string url, T jsonBody) where T : new()
		{
			url = GetFullPath(url);
			RestRequest request = new RestRequest(url, Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(jsonBody);
			IRestResponse response = Client.ExecuteAsPost<T>(request, "POST");

			return new WebApiResponse()
			{
				Url = Client.BuildUri(request).ToString(), 
				Content = response.Content, 
				HttpStatusCode = response.StatusCode 
			};
		}

		public WebApiResponse Put<T>(string url, T jsonBody) where T : new()
		{
			url = GetFullPath(url);
			RestRequest request = new RestRequest(url, Method.POST);
			request.RequestFormat = DataFormat.Json;
			request.AddBody(jsonBody);
			IRestResponse response = Client.ExecuteAsPost<T>(request, "PUT");

			return new WebApiResponse()
			{
				Url = Client.BuildUri(request).ToString(), 
				Content = response.Content, 
				HttpStatusCode = response.StatusCode 
			};
		}
	}
}
