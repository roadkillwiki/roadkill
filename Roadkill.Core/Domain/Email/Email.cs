using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

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
		/// <param name="plainTextView">The plain text view.</param>
		/// <param name="htmlView">The HTML view.</param>
		public Email(UserSummary summary,string plainTextView,string htmlView)
		{
			if (summary == null || (string.IsNullOrEmpty(summary.ExistingEmail) && string.IsNullOrEmpty(summary.NewEmail)))
				throw new EmailException(null, "The UserSummary for the email is null or has an empty email");

			if (string.IsNullOrEmpty(plainTextView))
				throw new EmailException(null, "No plain text view can be found for {0}", GetType().Name);

			if (string.IsNullOrEmpty(htmlView))
				throw new EmailException(null, "No HTML view can be found for {0}", GetType().Name);

			UserSummary = summary;
			PlainTextView = plainTextView;
			HtmlView = htmlView;
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
			HtmlView = HtmlView.Replace("{SITEURL}", RoadkillSettings.SiteUrl);
			HtmlView = HtmlView.Replace("{ACTIVATIONKEY}", summary.ActivationKey);
			HtmlView = HtmlView.Replace("{RESETKEY}", summary.PasswordResetKey);
			HtmlView = HtmlView.Replace("{USERID}", summary.Id.ToString());
			HtmlView = HtmlView.Replace("{SITENAME}", RoadkillSettings.SiteName);
			HtmlView = HtmlView.Replace("{REQUEST_IP}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);

			PlainTextView = PlainTextView.Replace("{FIRSTNAME}", summary.Firstname);
			PlainTextView = PlainTextView.Replace("{LASTNAME}", summary.Lastname);
			PlainTextView = PlainTextView.Replace("{EMAIL}", summary.NewEmail);
			PlainTextView = PlainTextView.Replace("{USERNAME}", summary.NewUsername);
			PlainTextView = PlainTextView.Replace("{SITEURL}", RoadkillSettings.SiteUrl);
			PlainTextView = PlainTextView.Replace("{ACTIVATIONKEY}", summary.ActivationKey);
			PlainTextView = PlainTextView.Replace("{RESETKEY}", summary.PasswordResetKey);
			PlainTextView = PlainTextView.Replace("{USERID}", summary.Id.ToString());
			PlainTextView = PlainTextView.Replace("{SITENAME}", RoadkillSettings.SiteName);
			PlainTextView = PlainTextView.Replace("{REQUEST_IP}", HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"]);
		}

		/// <summary>
		/// Sends a notification email to the provided address, using the template provided.
		/// </summary>
		/// <param name="toAddress"></param>
		/// <param name="summary"></param>
		/// <param name="emailTemplate"></param>
		public static void Send(Email emailTemplate)
		{
			string emailTo = emailTemplate.UserSummary.ExistingEmail;
			if (string.IsNullOrEmpty(emailTo))
				emailTo = emailTemplate.UserSummary.NewEmail;

			if (string.IsNullOrEmpty(emailTo))
				throw new EmailException(null, "The UserSummary has an empty current or new email address");

			// Construct the message and the two views
			MailMessage message = new MailMessage();
			message.To.Add(emailTo);
			message.Subject = "Please confirm your email address";
			
			AlternateView htmlView = AlternateView.CreateAlternateViewFromString(emailTemplate.HtmlView, new ContentType("text/html"));
			AlternateView plainTextView = AlternateView.CreateAlternateViewFromString(emailTemplate.PlainTextView, new ContentType("text/plain"));		
			message.AlternateViews.Add(htmlView);
			message.AlternateViews.Add(plainTextView);

			// Send + auth with the SMTP server if needed.
			SmtpClient client = new SmtpClient();
			client.Send(message);
		}
	}
}
