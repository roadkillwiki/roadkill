using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using NUnit.Framework;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Integration.WebApi
{
	[TestFixture]
	[Category("Integration")]
	public class UserControllerTests : WebApiTestBase
	{
		[Test]
		public void Authenticate_Should_Return_True_For_Known_User()
		{
			// Arrange
			string url = GetFullUrl("Authenticate");

			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = ADMIN_EMAIL,
				Password = ADMIN_PASSWORD
			};

			HttpClient client = new HttpClient();

			// Act
			HttpResponseMessage response = client.PostAsJsonAsync<UserController.UserInfo>(url, info).Result;
			string jsonResponse = response.Content.ReadAsStringAsync().Result;

			// Assert
			Assert.That(jsonResponse, Is.EqualTo("true"), jsonResponse);
		}

		[Test]
		public void Authenticate_Should_Return_False_For_Unknown_User()
		{
			// Arrange
			string url = GetFullUrl("Authenticate");

			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = "anyone@localhost",
				Password = ADMIN_PASSWORD
			};

			HttpClient client = new HttpClient();

			// Act
			HttpResponseMessage response = client.PostAsJsonAsync<UserController.UserInfo>(url, info).Result;
			string jsonResponse = response.Content.ReadAsStringAsync().Result;

			// Assert
			Assert.That(jsonResponse, Is.EqualTo("false"), jsonResponse);
		}

		[Test]
		public void GetUsers_Should_Return_All_Users()
		{
			// Arrange
			HttpClient client = Login();
			string url = GetFullUrl("User");

			// Act
			HttpResponseMessage response = client.GetAsync(url).Result;
			IEnumerable<UserViewModel> users = response.Content.ReadAsAsync<IEnumerable<UserViewModel>>().Result;

			// Assert
			Assert.That(users.Count(), Is.EqualTo(2), response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void GetUser_Should_Return_Admin_User()
		{
			// Arrange
			HttpClient client = Login();
			string url = GetFullUrl("User/" +ADMIN_ID);

			// Act
			HttpResponseMessage response = client.GetAsync(url).Result;
			UserViewModel user = response.Content.ReadAsAsync<UserViewModel>().Result;

			// Assert
			Assert.That(user, Is.Not.Null, response.Content.ReadAsStringAsync().Result);
			Assert.That(user.Id, Is.EqualTo(ADMIN_ID), response.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void Logout_Should_Remove_Auth_Cookie()
		{
			// Arrange
			string authUrl = GetFullUrl("Authenticate");
			string logoutUrl = GetFullUrl("Logout");
			string usersUrl = GetFullUrl("User");

			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = ADMIN_EMAIL,
				Password = ADMIN_PASSWORD
			};

			HttpClient client = new HttpClient();

			// Act
			HttpResponseMessage authResult = client.PostAsJsonAsync<UserController.UserInfo>(authUrl, info).Result;
			HttpResponseMessage logoutResult = client.GetAsync(logoutUrl).Result;
			HttpResponseMessage response = client.GetAsync(usersUrl).Result;

			// Assert
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), response.Content.ReadAsStringAsync().Result);
		}
	}
}
