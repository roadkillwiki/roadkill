using System.Collections.Generic;
using System.Configuration;
using System.Net;
using RestSharp;

namespace Roadkill.Tests.Acceptance.Headless.RestApi
{
	public class WebApiClient
	{
		public string BaseUrl { get; set; }
		public RestClient Client { get; set; }
		public string ApiKey { get; set; }

		public WebApiClient()
		{
			string url = ConfigurationManager.AppSettings["url"];
			if (string.IsNullOrEmpty(url))
				url = TestConstants.WEB_BASEURL;

			BaseUrl = url;

			Client = new RestClient(BaseUrl);
			Client.CookieContainer = new CookieContainer();

			ApiKey = TestConstants.REST_API_KEY;
		}

		public string GetFullPath(string fullPath)
		{
			return string.Format("/api/{0}", fullPath);
		}

		public WebApiResponse Get(string url, Dictionary<string, string> queryString = null)
		{
			url = GetFullPath(url);
			RestRequest request = new RestRequest(url, Method.GET);
			AddApiKey(request);

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
			AddApiKey(request);

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
			AddApiKey(request);

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
			AddApiKey(request);

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

		private void AddApiKey(RestRequest restRequest)
		{
			if (!string.IsNullOrEmpty(ApiKey))
				restRequest.AddHeader("Authorization", ApiKey);
		}
	}
}
