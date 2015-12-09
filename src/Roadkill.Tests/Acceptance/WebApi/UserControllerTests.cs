using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;
using Roadkill.Core.Mvc.Controllers.Api;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Acceptance.WebApi
{
	[TestFixture]
	[Category("Acceptance")]
	public class UserControllerTests : WebApiTestBase
	{
		[Test]
		public void authenticate_should_return_true_for_known_user()
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
		public void authenticate_should_return_false_for_unknown_user()
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
		public void getusers_should_return_all_users()
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
		public void getuser_should_return_admin_user()
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
		public void logout_should_remove_auth_cookie()
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
