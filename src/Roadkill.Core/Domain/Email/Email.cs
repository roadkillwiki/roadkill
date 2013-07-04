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

namespace Roadkill.Core
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
	public abstract class Email
	{
		protected ApplicationSettings ApplicationSettings;
		protected SiteSettings SiteSettings;

		/// <summary>
		/// The HTML template for the email.
		/// </summary>
		public string HtmlView { get; set; }

		/// <summary>
		/// The plain text template for the email.
		/// </summary>
		public string PlainTextView { get; set; }

		/// <summary>
		/// The user this email should be sent to.
		/// </summary>
		public UserSummary UserSummary { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Email"/> class.
		/// </summary>
		/// <param name="summary">The summary.</param>
		/// <param name="applicationSettings"></param>
		/// <param name="siteSettings"></param>
		public Email(ApplicationSettings applicationSettings, SiteSettings siteSettings)
		{
			ApplicationSettings = applicationSettings;
			SiteSettings = siteSettings;
		}

		/// <summary>
		/// Replaces all tokens in the html and plain text views.
		/// </summary>
		/// <param name="summary"></param>
		protected virtual string ReplaceTokens(UserSummary summary, string template)
		{
			string result = template;

			result = result.Replace("{FIRSTNAME}", summary.Firstname);
			result = result.Replace("{LASTNAME}", summary.Lastname);
			result = result.Replace("{EMAIL}", summary.NewEmail);
			result = result.Replace("{USERNAME}", summary.NewUsername);
			result = result.Replace("{SITEURL}", SiteSettings.SiteUrl);
			result = result.Replace("{ACTIVATIONKEY}", summary.ActivationKey);
			result = result.Replace("{RESETKEY}", summary.PasswordResetKey);
			result = result.Replace("{USERID}", summary.Id.ToString());
			result = result.Replace("{SITENAME}", SiteSettings.SiteName);

			if (HttpContext.Current != null)
				result = result.Replace("{REQUEST_IP}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);

			return result;
		}

		/// <summary>
		/// Sends a notification email to the provided address, using the template provided.
		/// </summary>
		/// <param name="emailTemplate"></param>
		public virtual void Send(UserSummary summary)
		{
			if (summary == null || (string.IsNullOrEmpty(summary.ExistingEmail) && string.IsNullOrEmpty(summary.NewEmail)))
				throw new EmailException(null, "The UserSummary for the email is null or has an empty email");

			if (string.IsNullOrEmpty(PlainTextView))
				throw new EmailException(null, "No plain text view can be found for {0}", GetType().Name);

			if (string.IsNullOrEmpty(HtmlView))
				throw new EmailException(null, "No HTML view can be found for {0}", GetType().Name);

			string plainTextContent = ReplaceTokens(summary, PlainTextView);
			string htmlContent = ReplaceTokens(summary, HtmlView);

			string emailTo = UserSummary.ExistingEmail;
			if (string.IsNullOrEmpty(emailTo))
				emailTo = UserSummary.NewEmail;

			if (string.IsNullOrEmpty(emailTo))
				throw new EmailException(null, "The UserSummary has an empty current or new email address");

			// Construct the message and the two views
			MailMessage message = new MailMessage();
			message.To.Add(emailTo);
			message.Subject = "Please confirm your email address";
			
			AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(plainTextContent, new ContentType("text/plain"));
			AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlContent, new ContentType("text/html"));
			message.AlternateViews.Add(htmlView);
			message.AlternateViews.Add(plainTextView);

			// Send + auth with the SMTP server if needed.
			SmtpClient client = new SmtpClient();
			
			// Add "~" support for pickupdirectories.
			if (client.DeliveryMethod == SmtpDeliveryMethod.SpecifiedPickupDirectory && client.PickupDirectoryLocation.StartsWith("~"))
			{
				string root = AppDomain.CurrentDomain.BaseDirectory;
				string pickupRoot = client.PickupDirectoryLocation.Replace("~/", root);
				pickupRoot = pickupRoot.Replace("/",@"\");
				client.PickupDirectoryLocation = pickupRoot;
			}
			client.Send(message);
		}

		/// <summary>
		/// Reads the text file prowvided from the email templates directory.
		/// If a culture-specific version of the file exists, e.g. /fr/signup.txt then this is used instead.
		/// </summary>
		protected string ReadTemplateFile(string filename)
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
	}
}
