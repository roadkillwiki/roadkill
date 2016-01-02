using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Acceptance.Headless.RestApi
{
	[TestFixture]
	[Category("Acceptance")]
	public class UserControllerTests : WebApiTestBase
	{
		[Test]
		public void getusers_should_return_all_users()
		{
			// Arrange
			WebApiClient apiclient = new WebApiClient();

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

			// Act
			WebApiResponse<UserViewModel> response = apiclient.Get<UserViewModel>("User", queryString);

			// Assert
			UserViewModel userViewModel = response.Result;
			Assert.That(userViewModel, Is.Not.Null, response);
			Assert.That(userViewModel.Id, Is.EqualTo(ADMIN_ID), response);
		}
	}
}
