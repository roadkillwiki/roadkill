using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Net.Mime;

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
		/// Initializes a new instance of the <see cref="Email"/> class.
		/// </summary>
		/// <param name="summary">The summary.</param>
		/// <param name="plainTextView">The plain text view.</param>
		/// <param name="htmlView">The HTML view.</param>
		public Email(UserSummary summary,string plainTextView,string htmlView)
		{
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
			if (string.IsNullOrEmpty(PlainTextView))
				throw new EmailException(null, "No plain text view can be found for {0}", GetType().Name);

			if (string.IsNullOrEmpty(HtmlView))
				throw new EmailException(null, "No HTML view can be found for {0}", GetType().Name);

			HtmlView = HtmlView.Replace("{FIRSTNAME}", summary.Firstname);
			HtmlView = HtmlView.Replace("{LASTNAME}", summary.Lastname);
			HtmlView = HtmlView.Replace("{EMAIL}", summary.NewEmail);
			HtmlView = HtmlView.Replace("{SITEURL}", RoadkillSettings.SiteUrl);
			HtmlView = HtmlView.Replace("{ACTIVATIONKEY}", summary.ActivationKey);
			HtmlView = HtmlView.Replace("{USERID}", summary.Id.ToString());
			HtmlView = HtmlView.Replace("{SITENAME}", RoadkillSettings.SiteName);

			PlainTextView = PlainTextView.Replace("{FIRSTNAME}", summary.Firstname);
			PlainTextView = PlainTextView.Replace("{LASTNAME}", summary.Lastname);
			PlainTextView = PlainTextView.Replace("{EMAIL}", summary.NewEmail);
			PlainTextView = PlainTextView.Replace("{SITEURL}", RoadkillSettings.SiteUrl);
			PlainTextView = PlainTextView.Replace("{ACTIVATIONKEY}", summary.ActivationKey);
			PlainTextView = PlainTextView.Replace("{USERID}", summary.Id.ToString());
			PlainTextView = PlainTextView.Replace("{SITENAME}", RoadkillSettings.SiteName);
		}

		/// <summary>
		/// Sends a notification email to the provided address, using the template provided.
		/// </summary>
		/// <param name="toAddress"></param>
		/// <param name="summary"></param>
		/// <param name="emailTemplate"></param>
		public static void Send(string toAddress,UserSummary summary,Email emailTemplate)
		{
			// Construct the message and the two views
			MailMessage message = new MailMessage();
			message.To.Add(toAddress);
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
