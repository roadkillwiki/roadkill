using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.LightSpeed;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Integration.WebApi
{
	[TestFixture]
	[Category("Integration")]
	public class PageControllerTests : WebApiTestBase
	{
		[Test]
		public void Get_Should_Return_All_Pages()
		{
			// Arrange
			AddPage("test", "this is page 1");
			AddPage("page 2", "this is page 2");

			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse<List<PageViewModel>> response = apiclient.Get<List<PageViewModel>>("Pages");

			// Assert
			IEnumerable<PageViewModel> pages = response.Result;
			Assert.That(pages.Count(), Is.EqualTo(2), response);
		}

		[Test]
		public void Get_With_Id_Should_Return_Correct_Page()
		{
			// Arrange
			PageContent expectedPage = AddPage("test", "this is page 1");
			var queryString = new Dictionary<string, string>()
			{ 
				{ "Id", expectedPage.Page.Id.ToString() }
			};
			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse<PageViewModel> response = apiclient.Get<PageViewModel>("Pages", queryString);

			// Assert
			PageViewModel actualPage = response.Result;
			Assert.That(actualPage, Is.Not.Null, response.ToString());
			Assert.That(actualPage.Id, Is.EqualTo(expectedPage.Page.Id), response.ToString());
		}

		[Test]
		public void Post_Should_Add_Page()
		{
			// Arrange
			PageViewModel page = new PageViewModel()
			{
				Title = "Hello",
				CreatedBy = "admin",
				CreatedOn = DateTime.UtcNow,
				Content = "some content",
				RawTags = "tag1,tag2"
			};

			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse response = apiclient.Post<PageViewModel>("Pages", page);

			// Assert
			IRepository repository = GetRepository();
			IEnumerable<Page> pages = repository.AllPages();
			Assert.That(pages.Count(), Is.EqualTo(1), response);
		}

		[Test]
		public void Put_Should_Update_Page()
		{
			// Arrange
			PageContent pageContent = AddPage("test", "this is page 1");
			PageViewModel viewModel = new PageViewModel(pageContent.Page);
			viewModel.Title = "New title";
			
			WebApiClient apiclient = new WebApiClient();
			apiclient.Login();

			// Act
			WebApiResponse response = apiclient.Put<PageViewModel>("Pages/Put", viewModel);

			// Assert
			IRepository repository = GetRepository();
			Page page = repository.AllPages().FirstOrDefault();
			Assert.That(page.Title, Is.EqualTo("New title"), response);
		}
	}
}
