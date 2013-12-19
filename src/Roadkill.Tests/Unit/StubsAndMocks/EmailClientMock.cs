using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Roadkill.Core.Email;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class EmailClientMock : IEmailClient
	{
		public string PickupDirectoryLocation { get; set; }
		public SmtpDeliveryMethod DeliveryMethod { get; set; }
		public bool Sent { get; set; }
		public MailMessage Message { get; set; }

		public void Send(MailMessage message)
		{
			Sent = true;
			Message = message;
		}

		public SmtpDeliveryMethod GetDeliveryMethod()
		{
			return DeliveryMethod;
		}
	}
}
