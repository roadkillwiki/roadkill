using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;
using Roadkill.Core.Configuration;
using System.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using System.IO;
using System.Globalization;
using Roadkill.Core.Database;

namespace Roadkill.Core.Email
{
	/// <summary>
	/// A base class for an email template.
	/// </summary>
	/// <remarks>
	/// The following tokens are replaced inside the email templates:
	/// - {FIRSTNAME}
	/// - {LASTNAME}
	/// - {EMAIL}
	/// - {SITEURL}
	/// - {ACTIVATIONKEY}
	/// - {USERID}
	/// - {SITENAME}
	/// </remarks>
	public abstract class EmailTemplate
	{
		protected ApplicationSettings ApplicationSettings;
		protected SiteSettings SiteSettings;
		protected IEmailClient EmailClient;
		protected IRepository Repository;

		/// <summary>
		/// The HTML template for the email.
		/// </summary>
		public string HtmlView { get; set; }

		/// <summary>
		/// The plain text template for the email.
		/// </summary>
		public string PlainTextView { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="EmailTemplate"/> class.
		/// </summary>
		/// <param name="applicationSettings">Application wide settings</param>
		/// <param name="repository">The repository retrieve the site settings from</param>
		/// <param name="emailClient">The <see cref="IEmailClient"/> to send the mail through. If this 
		/// parameter is null, then <see cref="EmailClient"/> is used</param>
		protected EmailTemplate(ApplicationSettings applicationSettings, IRepository repository, IEmailClient emailClient)
		{
			if (applicationSettings == null)
				throw new ArgumentNullException("applicationSettings");

			if (repository == null)
				throw new ArgumentNullException("repository");

			ApplicationSettings = applicationSettings;
			Repository = repository;
			
			EmailClient = emailClient;
			if (EmailClient == null)
				EmailClient = new EmailClient();
		}

		/// <summary>
		/// Sends a notification email to the provided address, using the template provided.
		/// </summary>
		public virtual void Send(UserViewModel model)
		{
			if (model == null || (string.IsNullOrEmpty(model.ExistingEmail) && string.IsNullOrEmpty(model.NewEmail)))
				throw new EmailException(null, "The UserViewModel for the email is null or has an empty email");

			if (string.IsNullOrEmpty(PlainTextView))
				throw new EmailException(null, "No plain text view can be found for {0}", GetType().Name);

			if (string.IsNullOrEmpty(HtmlView))
				throw new EmailException(null, "No HTML view can be found for {0}", GetType().Name);

			string plainTextContent = ReplaceTokens(model, PlainTextView);
			string htmlContent = ReplaceTokens(model, HtmlView);

			string emailTo = model.ExistingEmail;
			if (string.IsNullOrEmpty(emailTo))
				emailTo = model.NewEmail;

			if (string.IsNullOrEmpty(emailTo))
				throw new EmailException(null, "The UserViewModel has an empty current or new email address");

			// Construct the message and the two views
			MailMessage message = new MailMessage();
			message.To.Add(emailTo);
			message.Subject = "Please confirm your email address";
			
			AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(plainTextContent, new ContentType("text/plain"));
			AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlContent, new ContentType("text/html"));
			message.AlternateViews.Add(htmlView);
			message.AlternateViews.Add(plainTextView);

			// Add "~" support for pickupdirectories.
			if (EmailClient.GetDeliveryMethod() == SmtpDeliveryMethod.SpecifiedPickupDirectory && 
				!string.IsNullOrEmpty(EmailClient.PickupDirectoryLocation) &&
				EmailClient.PickupDirectoryLocation.StartsWith("~"))
			{
				string root = AppDomain.CurrentDomain.BaseDirectory;
				string pickupRoot = EmailClient.PickupDirectoryLocation.Replace("~/", root);
				pickupRoot = pickupRoot.Replace("/", @"\");
				EmailClient.PickupDirectoryLocation = pickupRoot;
			}

			EmailClient.Send(message);
		}

		/// <summary>
		/// Reads the text file prowvided from the email templates directory.
		/// If a culture-specific version of the file exists, e.g. /fr/signup.txt then this is used instead.
		/// </summary>
		protected internal string ReadTemplateFile(string filename)
		{
			string templatePath = ApplicationSettings.EmailTemplateFolder;
			string textfilePath = Path.Combine(templatePath, filename);
			string culturePath = Path.Combine(templatePath, CultureInfo.CurrentUICulture.Name);

			// If there's templates for the current culture, then use those instead
			if (Directory.Exists(culturePath))
			{
				string culturePlainTextFile = Path.Combine(culturePath, filename);
				if (File.Exists(culturePlainTextFile))
					textfilePath = culturePlainTextFile;
			}

			return File.ReadAllText(textfilePath);
		}

		/// <summary>
		/// Replaces all tokens in the html and plain text views.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="template"></param>
		protected internal virtual string ReplaceTokens(UserViewModel model, string template)
		{
			if (SiteSettings == null)
				SiteSettings = Repository.GetSiteSettings();

			string result = template;

			result = result.Replace("{FIRSTNAME}", model.Firstname);
			result = result.Replace("{LASTNAME}", model.Lastname);
			result = result.Replace("{EMAIL}", model.NewEmail);
			result = result.Replace("{USERNAME}", model.NewUsername);
			result = result.Replace("{SITEURL}", SiteSettings.SiteUrl);
			result = result.Replace("{ACTIVATIONKEY}", model.ActivationKey);
			result = result.Replace("{RESETKEY}", model.PasswordResetKey);
			result = result.Replace("{USERID}", model.Id.ToString());
			result = result.Replace("{SITENAME}", SiteSettings.SiteName);

			if (HttpContext.Current != null)
				result = result.Replace("{REQUEST_IP}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);

			return result;
		}
	}
}
