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
			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = ADMIN_EMAIL,
				Password = ADMIN_PASSWORD
			};

			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse response = apiclient.Post<UserController.UserInfo>("Authenticate", info);

			// Assert
			Assert.That(response.Content, Is.EqualTo("true"), response);
		}

		[Test]
		public void Authenticate_Should_Return_False_For_Unknown_User()
		{
			// Arrange
			UserController.UserInfo info = new UserController.UserInfo()
			{
				Email = "badlogin@localhost",
				Password = ADMIN_PASSWORD
			};

			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse response = apiclient.Post<UserController.UserInfo>("Authenticate", info);

			// Assert
			Assert.That(response.Content, Is.EqualTo("false"), response);
		}

		[Test]
		public void GetUsers_Should_Return_All_Users()
		{
			// Arrange
			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse<List<UserViewModel>> response = apiclient.Get<List<UserViewModel>>("User");

			// Assert
			List<UserViewModel> results = response.Result;
			Assert.That(results.Count(), Is.EqualTo(2), response);
		}

		[Test]
		public void GetUser_Should_Return_Admin_User()
		{
			// Arrange
			var queryString = new Dictionary<string, string>()
			{ 
				{ "Id", ADMIN_ID.ToString() }
			};

			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();	

			// Act
			WebApiResponse<UserViewModel> response = apiclient.Get<UserViewModel>("User", queryString);

			// Assert
			UserViewModel userViewModel = response.Result;
			Assert.That(userViewModel, Is.Not.Null, response);
			Assert.That(userViewModel.Id, Is.EqualTo(ADMIN_ID), response);
		}

		[Test]
		public void Logout_Should_Remove_Auth_Cookie()
		{
			// Arrange
			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();
			apiclient.Get("Logout");
			apiclient.Get("User");

			// Act
			apiclient.Login();
			apiclient.Get("Logout");
			WebApiResponse response = apiclient.Get("User");

			// Assert
			Assert.That(response.HttpStatusCode, Is.EqualTo(HttpStatusCode.Unauthorized), response);
		}
	}
}
