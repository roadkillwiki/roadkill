using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using RestSharp;

namespace Roadkill.Tests.Acceptance.Headless.RestApi
{
	[TestFixture]
	[Category("Acceptance")]
	public class ApiKeysTests : WebApiTestBase
	{
		private static void RemoveApiKeys()
		{
			try
			{
				string sitePath = TestConstants.WEB_PATH;
				string roadkillConfigPath = Path.Combine(sitePath, "Roadkill.config");
				string configText = File.ReadAllText(roadkillConfigPath);

				configText = configText.Replace(@"apiKeys=""apikey1,apikey2""", "");
				File.WriteAllText(roadkillConfigPath, configText);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		[Test]
		public void wrong_apikey_in_header_should_return_401_unauthorized()
		{
			// Arrange
			WebApiClient apiclient = new WebApiClient();
			apiclient.ApiKey = "bad api key";

			// Act
			WebApiResponse response = apiclient.Get("User");

			// Assert
			Assert.That(response.HttpStatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), response);
		}

		[Test]
		public void missing_apikey_in_header_should_return_400_badrequest()
		{
			// Arrange
			WebApiClient apiclient = new WebApiClient();
			apiclient.ApiKey = "";

			// Act
			WebApiResponse response = apiclient.Get("User");

			// Assert
			Assert.That(response.HttpStatusCode, Is.EqualTo(HttpStatusCode.BadRequest), response);
		}

		[Test]
		public void blank_apikeys_config_setting_should_disable_rest_api()
		{
			// Arrange
			RemoveApiKeys();
			WebApiClient apiclient = new WebApiClient();

			// Act
			WebApiResponse response = apiclient.Get("User");

			// Assert
			Assert.That(response.HttpStatusCode, Is.EqualTo(HttpStatusCode.NotFound), response);
		}

		[Test]
		public void blank__apikeys_config_setting_should_disable_swashbuckle()
		{
			// Arrange
			RemoveApiKeys();
			var client = new RestClient(TestConstants.WEB_BASEURL);
			var restRequest = new RestRequest("/swagger/");

			// Act
			IRestResponse response = client.Get(restRequest);

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), response.Content);
		}
	}
}
