using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Tests.Acceptance;

namespace Roadkill.Tests.Integration.WebApi
{
	[TestFixture]
	[Category("Unit")]
	public class UserTests : WebApiTestBase
	{
		[Test]
		public void Authenticate_Should_Return_True_For_Known_User()
		{
			// Arrange
			string url = GetUrl("Authenticate");

			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = ADMIN_EMAIL,
				Password = ADMIN_PASSWORD
			};

			HttpClient client = new HttpClient();

			// Act
			var result = client.PostAsJsonAsync<UserController.UserInfo>(url, info).Result;
			string jsonResponse = result.Content.ReadAsStringAsync().Result;

			// Assert
			Assert.That(jsonResponse, Is.EqualTo("true"));
		}

		[Test]
		public void Authenticate_Should_Return_True_For_Unknown_User()
		{
			// Arrange
			string url = GetUrl("Authenticate");

			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = "anyone@localhost",
				Password = ADMIN_PASSWORD
			};

			HttpClient client = new HttpClient();

			// Act
			var result = client.PostAsJsonAsync<UserController.UserInfo>(url, info).Result;
			string jsonResponse = result.Content.ReadAsStringAsync().Result;

			// Assert
			Assert.That(jsonResponse, Is.EqualTo("false"));
		}

		[Test]
		public void Logout_Should_Remove_Auth_Cookie()
		{
			// Arrange
			string authUrl = GetUrl("Authenticate");
			string logoutUrl = GetUrl("Logout");
			string usersUrl = GetUrl("User");

			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = ADMIN_EMAIL,
				Password = ADMIN_PASSWORD
			};

			HttpClient client = new HttpClient();

			// Act
			var x = client.PostAsJsonAsync<UserController.UserInfo>(authUrl, info).Result;
			var y = client.GetAsync(logoutUrl).Result;
			HttpContent content = client.GetAsync(usersUrl).Result.Content;

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(401));
		}
	}
}
