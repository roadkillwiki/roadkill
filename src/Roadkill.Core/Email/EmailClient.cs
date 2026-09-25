using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Email
{
	/// <summary>
	/// Sends emails using the SMTP settings from the Roadkill configuration. If no SMTP host is configured,
	/// the emails are written to the pickup directory (~/App_Data/TempSmtp by default).
	/// </summary>
	public class EmailClient : IEmailClient
	{
		private readonly SmtpSettings _smtpSettings;
		private readonly ApplicationSettings _applicationSettings;

		public string PickupDirectoryLocation { get; set; }

		public EmailClient(ApplicationSettings applicationSettings)
		{
			_applicationSettings = applicationSettings;
			_smtpSettings = applicationSettings.Smtp ?? new SmtpSettings();
			PickupDirectoryLocation = _smtpSettings.PickupDirectory;
		}

		public void Send(MailMessage message)
		{
			if (message.From == null && !string.IsNullOrEmpty(_smtpSettings.From))
				message.From = new MailAddress(_smtpSettings.From);

			using (SmtpClient smtpClient = CreateSmtpClient())
			{
				smtpClient.Send(message);
			}
		}

		public SmtpDeliveryMethod GetDeliveryMethod()
		{
			return string.IsNullOrEmpty(_smtpSettings.Host) ? SmtpDeliveryMethod.SpecifiedPickupDirectory : SmtpDeliveryMethod.Network;
		}

		private SmtpClient CreateSmtpClient()
		{
			SmtpClient smtpClient = new SmtpClient();

			if (GetDeliveryMethod() == SmtpDeliveryMethod.Network)
			{
				smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
				smtpClient.Host = _smtpSettings.Host;
				smtpClient.Port = _smtpSettings.Port;
				smtpClient.EnableSsl = _smtpSettings.EnableSsl;

				if (!string.IsNullOrEmpty(_smtpSettings.Username))
					smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
			}
			else
			{
				string pickupDirectory = _applicationSettings.MapPath(PickupDirectoryLocation);
				if (!Directory.Exists(pickupDirectory))
					Directory.CreateDirectory(pickupDirectory);

				smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
				smtpClient.PickupDirectoryLocation = pickupDirectory;
			}

			return smtpClient;
		}
	}
}
