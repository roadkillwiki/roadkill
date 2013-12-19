using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Core.Email
{
	public class EmailClient : IEmailClient
	{
		private SmtpClient _smtpClient;
		public string PickupDirectoryLocation { get; set; }

		public EmailClient()
		{
			_smtpClient = new SmtpClient();

			// Default it to the SmtpClient's settings, which are read from a .config
			PickupDirectoryLocation = _smtpClient.PickupDirectoryLocation;
		}

		public void Send(MailMessage message)
		{
			_smtpClient.PickupDirectoryLocation = PickupDirectoryLocation;
			_smtpClient.Send(message);
		}

		public SmtpDeliveryMethod GetDeliveryMethod()
		{
			return _smtpClient.DeliveryMethod;
		}
	}
}
