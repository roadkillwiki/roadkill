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
		public Email(UserSummary summary, ApplicationSettings applicationSettings, SiteSettings siteSettings)
		{
			if (summary == null || (string.IsNullOrEmpty(summary.ExistingEmail) && string.IsNullOrEmpty(summary.NewEmail)))
				throw new EmailException(null, "The UserSummary for the email is null or has an empty email");

			UserSummary = summary;
			ApplicationSettings = applicationSettings;
			SiteSettings = siteSettings;

			ReplaceTokens(summary);	
		}

		/// <summary>
		/// Replaces all tokens in the html and plain text views.
		/// </summary>
		/// <param name="summary"></param>
		protected virtual void ReplaceTokens(UserSummary summary)
		{
			HtmlView = HtmlView.Replace("{FIRSTNAME}", summary.Firstname);
			HtmlView = HtmlView.Replace("{LASTNAME}", summary.Lastname);
			HtmlView = HtmlView.Replace("{EMAIL}", summary.NewEmail);
			HtmlView = HtmlView.Replace("{USERNAME}", summary.NewUsername);
			HtmlView = HtmlView.Replace("{SITEURL}", SiteSettings.SiteUrl);
			HtmlView = HtmlView.Replace("{ACTIVATIONKEY}", summary.ActivationKey);
			HtmlView = HtmlView.Replace("{RESETKEY}", summary.PasswordResetKey);
			HtmlView = HtmlView.Replace("{USERID}", summary.Id.ToString());
			HtmlView = HtmlView.Replace("{SITENAME}", SiteSettings.SiteName);
			HtmlView = HtmlView.Replace("{REQUEST_IP}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);

			PlainTextView = PlainTextView.Replace("{FIRSTNAME}", summary.Firstname);
			PlainTextView = PlainTextView.Replace("{LASTNAME}", summary.Lastname);
			PlainTextView = PlainTextView.Replace("{EMAIL}", summary.NewEmail);
			PlainTextView = PlainTextView.Replace("{USERNAME}", summary.NewUsername);
			PlainTextView = PlainTextView.Replace("{SITEURL}", SiteSettings.SiteUrl);
			PlainTextView = PlainTextView.Replace("{ACTIVATIONKEY}", summary.ActivationKey);
			PlainTextView = PlainTextView.Replace("{RESETKEY}", summary.PasswordResetKey);
			PlainTextView = PlainTextView.Replace("{USERID}", summary.Id.ToString());
			PlainTextView = PlainTextView.Replace("{SITENAME}", SiteSettings.SiteName);
			PlainTextView = PlainTextView.Replace("{REQUEST_IP}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
		}

		/// <summary>
		/// Sends a notification email to the provided address, using the template provided.
		/// </summary>
		/// <param name="emailTemplate"></param>
		public virtual void Send()
		{
			if (string.IsNullOrEmpty(PlainTextView))
				throw new EmailException(null, "No plain text view can be found for {0}", GetType().Name);

			if (string.IsNullOrEmpty(HtmlView))
				throw new EmailException(null, "No HTML view can be found for {0}", GetType().Name);

			string emailTo = UserSummary.ExistingEmail;
			if (string.IsNullOrEmpty(emailTo))
				emailTo = UserSummary.NewEmail;

			if (string.IsNullOrEmpty(emailTo))
				throw new EmailException(null, "The UserSummary has an empty current or new email address");

			// Construct the message and the two views
			MailMessage message = new MailMessage();
			message.To.Add(emailTo);
			message.Subject = "Please confirm your email address";
			
			AlternateView htmlView = AlternateView.CreateAlternateViewFromString(HtmlView, new ContentType("text/html"));
			AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(PlainTextView, new ContentType("text/plain"));		
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
		/// Reads the text file provided from the email templates directory.
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
