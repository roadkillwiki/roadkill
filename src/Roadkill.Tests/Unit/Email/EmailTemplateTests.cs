using System;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Email;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Services;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class EmailTemplateTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private SiteSettings _siteSettings;
		private EmailClientMock _emailClientMock;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_siteSettings = _container.SettingsService.GetSiteSettings();
			_emailClientMock = _container.EmailClient;
		}
		
		[Test]
		public void Send_Should_Use_EmailClient_To_Send()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _siteSettings, _emailClientMock);
			UserViewModel userModel = new UserViewModel();
			userModel.ExistingEmail = "someone@localhost";
			userModel.NewEmail = "someone@localhost";

			// Act
			emailTemplate.Send(userModel);

			// Assert
			Assert.That(_emailClientMock.Sent, Is.True);
		}

		// Should_Use_SmtpClient_When_No_Client_In_Constructor
		// Send_Should_Use_EmailClient_To_Send
		// Send_Should_Throw_EmailException_When_Model_Is_Null
		// Send_Should_Throw_EmailException_When_Model_Email_And_NewEmail_Is_Empty
		// Send_Should_Throw_EmailException_When_PlainTextView_Is_Empty
		// Send_Should_Throw_EmailException_When_HtmlView_Is_Empty
		// Send_Should_Set_Two_Alternative_Views
		// Send_Should_Change_PickupDirectory_To_AppDomainRoot_When_Starting_With_~_And_DeliveryType_Is_PickupLocation

		// ReadTemplateFile_Should_Read_Textfile_Contents
		// ReadTemplateFile_Should_Read_Culture_Textfile_Contents

		// ReplaceTokens_Should_Replace_All_Tokens_From_Model

		// ResetPasswordEmail_Send_Should_Use_ResetPassword_Txt_And_Html_File_Templates
		// SignupEmail_Send_Should_Use_ResetPassword_Txt_And_Html_File_Templates
	}
}
