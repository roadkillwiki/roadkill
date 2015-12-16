using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Email;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit.Email
{
	[TestFixture]
	[Category("Unit")]
	public class EmailTemplateTests
	{
		private MocksAndStubsContainer _container;

		private ApplicationSettings _applicationSettings;
		private IRepository _repository;
		private EmailClientMock _emailClientMock;

		[SetUp]
		public void Setup()
		{
			_container = new MocksAndStubsContainer();

			_applicationSettings = _container.ApplicationSettings;
			_repository = _container.Repository;
			_emailClientMock = _container.EmailClient;
		}

		[Test]
		public void should_use_default_smtpclient_when_client_is_null_in_constructor()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, null);

			// Act +  Assert
			Assert.That(emailTemplate.GetEmailClient(), Is.TypeOf<EmailClient>());
		}
		
		[Test]
		public void send_should_use_emailclient_to_send()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);
			UserViewModel userModel = new UserViewModel();
			userModel.ExistingEmail = "someone@localhost";
			userModel.NewEmail = "someone@localhost";

			// Act
			emailTemplate.Send(userModel);

			// Assert
			Assert.That(_emailClientMock.Sent, Is.True);
		}

		[Test]
		[ExpectedException(typeof(EmailException))]
		public void Send_Should_Throw_EmailException_When_Model_Is_Null()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);
			UserViewModel userModel = null;

			// Act + Assert
			emailTemplate.Send(userModel);
		}

		[Test]
		[ExpectedException(typeof(EmailException))]
		public void Send_Should_Throw_EmailException_When_Model_Email_And_NewEmail_Is_Empty()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);
			UserViewModel userModel = new UserViewModel();
			userModel.ExistingEmail = null;
			userModel.NewEmail = "";

			// Act + Assert
			emailTemplate.Send(userModel);
		}

		[Test]
		[ExpectedException(typeof(EmailException))]
		public void Send_Should_Throw_EmailException_When_PlainTextView_Is_Empty()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);
			emailTemplate.PlainTextView = "";
			UserViewModel userModel = new UserViewModel();
			userModel.ExistingEmail = "someone@localhost";
			userModel.NewEmail = "someone@localhost";

			// Act + Assert
			emailTemplate.Send(userModel);
		}

		[Test]
		public void send_should_set_two_alternative_views_with_plaintext_and_html()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);
			UserViewModel userModel = new UserViewModel();
			userModel.ExistingEmail = "someone@localhost";
			userModel.NewEmail = "someone@localhost";

			// Act
			emailTemplate.Send(userModel);

			// Assert
			MailMessage message = _emailClientMock.Message;
			Assert.That(message.AlternateViews.Count, Is.EqualTo(2));

			AlternateView plainView = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType == "text/plain");
			AlternateView htmlView = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType == "text/html");

			Assert.That(plainView, Is.Not.Null);
			Assert.That(htmlView, Is.Not.Null);
		}

		[Test]
		public void send_should_change_pickupdirectory_to_appdomainroot_when_starting_with_virtualpath_and_deliverytype_is_pickuplocation()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);
			_emailClientMock.PickupDirectoryLocation = "~/App_Data/EmailDrop";
			_emailClientMock.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;

			UserViewModel userModel = new UserViewModel();
			userModel.ExistingEmail = "someone@localhost";
			userModel.NewEmail = "someone@localhost";

			// Act
			emailTemplate.Send(userModel);

			// Assert
			Assert.That(_emailClientMock.PickupDirectoryLocation, Is.StringStarting(AppDomain.CurrentDomain.BaseDirectory));
		}

		[Test]
		public void readtemplatefile_should_read_textfile_contents()
		{
			// Arrange
			_applicationSettings.EmailTemplateFolder = AppDomain.CurrentDomain.BaseDirectory;
			string expectedContents = DateTime.UtcNow.ToString();
			string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "emailtemplate.txt");
			File.WriteAllText(path, expectedContents);
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);

			// Act
			string actualContents = emailTemplate.ReadTemplateFile("emailtemplate.txt");

			// Assert
			Assert.That(actualContents, Is.EqualTo(expectedContents));
		}

		[Test]
		public void readtemplatefile_should_read_cultureui_textfile_contents()
		{
			// Arrange
			_applicationSettings.EmailTemplateFolder = AppDomain.CurrentDomain.BaseDirectory;
			string expectedContents = DateTime.UtcNow.ToString();
			string cultureDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fr-FR");
			if (!Directory.Exists(cultureDir))
				Directory.CreateDirectory(cultureDir);

			string cultureFilePath = Path.Combine(cultureDir, "emailtemplate.txt");
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");

			File.WriteAllText(cultureFilePath, expectedContents);
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);

			// Act
			string actualContents = emailTemplate.ReadTemplateFile("emailtemplate.txt");

			// Assert
			Assert.That(actualContents, Is.EqualTo(expectedContents));
		}

		[Test]
		public void replacetokens_should_replace_all_tokens_from_model()
		{
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);
			UserViewModel userModel = new UserViewModel();
			userModel.ActivationKey = "key";
			userModel.ExistingUsername = "username";
			userModel.Firstname = "firstname";
			userModel.Id = Guid.NewGuid();
			userModel.Lastname = "lastname";
			userModel.NewEmail = "NewEmail";
			userModel.PasswordResetKey = "resetkey";

			SiteSettings siteSettings = new SiteSettings();
			siteSettings.SiteName = "MySite";
			siteSettings.SiteUrl = "http://www.roadkillwiki.iz.de.biz";
			_repository.SaveSiteSettings(siteSettings);

			StringBuilder templateBuilder = new StringBuilder();
			templateBuilder.AppendLine("{FIRSTNAME}");
			templateBuilder.AppendLine("{LASTNAME}");
			templateBuilder.AppendLine("{EMAIL}");
			templateBuilder.AppendLine("{USERNAME}");
			templateBuilder.AppendLine("{ACTIVATIONKEY}");
			templateBuilder.AppendLine("{RESETKEY}");
			templateBuilder.AppendLine("{USERID}");
			templateBuilder.AppendLine("{SITENAME}");
			templateBuilder.AppendLine("{SITEURL}");

			StringBuilder expectedContent = new StringBuilder();
			expectedContent.AppendLine(userModel.Firstname);
			expectedContent.AppendLine(userModel.Lastname);
			expectedContent.AppendLine(userModel.NewEmail);
			expectedContent.AppendLine(userModel.NewUsername);
			expectedContent.AppendLine(userModel.ActivationKey);
			expectedContent.AppendLine(userModel.PasswordResetKey);
			expectedContent.AppendLine(userModel.Id.ToString());
			expectedContent.AppendLine(siteSettings.SiteName);
			expectedContent.AppendLine(siteSettings.SiteUrl);

			// Act
			string actualTemplate = emailTemplate.ReplaceTokens(userModel, templateBuilder.ToString());

			// Assert
			Assert.That(actualTemplate, Is.EqualTo(expectedContent.ToString()));
		}

		[Test]
		public void replacetokens_should_get_sitesettings_from_repository()
		{
			// Issue #229
			// Arrange
			EmailTemplateStub emailTemplate = new EmailTemplateStub(_applicationSettings, _repository, _emailClientMock);
			UserViewModel userModel = new UserViewModel();

			SiteSettings expectedSettings = new SiteSettings();
			expectedSettings.SiteName = "MySite";
			expectedSettings.SiteUrl = "http://www.roadkillwiki.iz.de.biz";
			_repository.SaveSiteSettings(expectedSettings);

			// Act
			emailTemplate.ReplaceTokens(userModel, "not used");
			SiteSettings actualSettings = emailTemplate.GetSiteSettings();

			// Assert
			Assert.That(actualSettings, Is.EqualTo(expectedSettings));
		}

		[Test]
		public void resetpasswordemail_send_should_read_resetpassword_txt_and_html_file_templates()
		{
			// Arrange
			_applicationSettings.EmailTemplateFolder = AppDomain.CurrentDomain.BaseDirectory;

			UserViewModel userModel = new UserViewModel();
			userModel.Id = Guid.NewGuid();
			userModel.NewEmail = "email@localhost";
			userModel.PasswordResetKey = "resetkey";

			string expectedPlainContents = "plain" +DateTime.UtcNow.ToString();
			string expectedHtmlContents = "html" + DateTime.UtcNow.ToString();
			CreateDummyTemplates("resetpassword", expectedPlainContents, expectedHtmlContents);

			ResetPasswordEmail resetPassword = new ResetPasswordEmail(_applicationSettings, _repository, _emailClientMock);

			// Act
			resetPassword.Send(userModel);

			// Assert
			MailMessage message = _emailClientMock.Message;
			Assert.That(message.AlternateViews.Count, Is.EqualTo(2));
			AssertAlternateViewContent(message, "text/plain", expectedPlainContents);
			AssertAlternateViewContent(message, "text/html", expectedHtmlContents);
		}

		[Test]
		public void signupemail_send_should_read_signup_txt_and_html_file_templates()
		{
			// Arrange
			_applicationSettings.EmailTemplateFolder = AppDomain.CurrentDomain.BaseDirectory;

			UserViewModel userModel = new UserViewModel();
			userModel.Id = Guid.NewGuid();
			userModel.NewEmail = "email@localhost";
			userModel.PasswordResetKey = "resetkey";

			string expectedPlainContents = "plain" + DateTime.UtcNow.ToString();
			string expectedHtmlContents = "html" + DateTime.UtcNow.ToString();
			CreateDummyTemplates("Signup", expectedPlainContents, expectedHtmlContents);

			SignupEmail signupEmail = new SignupEmail(_applicationSettings, _repository, _emailClientMock);

			// Act
			signupEmail.Send(userModel);

			// Assert
			MailMessage message = _emailClientMock.Message;
			Assert.That(message.AlternateViews.Count, Is.EqualTo(2));
			AssertAlternateViewContent(message, "text/plain", expectedPlainContents);
			AssertAlternateViewContent(message, "text/html", expectedHtmlContents);
		}

		private void AssertAlternateViewContent(MailMessage message, string contentType, string expectedContent)
		{
			AlternateView plainView = message.AlternateViews.FirstOrDefault(x => x.ContentType.MediaType == contentType);
			Assert.That(plainView, Is.Not.Null, "AlternateView does not containt a view with content type {0}", contentType);

			StreamReader reader = new StreamReader(plainView.ContentStream);
			Assert.That(reader.ReadToEnd(), Is.EqualTo(expectedContent), "AlternateView.ContentStream for {0} does not match expected content:", contentType);
		}

		private void CreateDummyTemplates(string filename, string plainContent, string htmlContent)
		{
			string plainPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename +".txt");
			File.WriteAllText(plainPath, plainContent);

			string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename +".html");
			File.WriteAllText(htmlPath, htmlContent);
		}
	}
}
