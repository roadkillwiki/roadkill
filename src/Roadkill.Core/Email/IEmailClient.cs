using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Core.Email
{
	/// <summary>
	/// 
	/// </summary>
	public interface IEmailClient
	{
		string PickupDirectoryLocation { get; set; }
		void Send(MailMessage message);
		SmtpDeliveryMethod GetDeliveryMethod();
	}
}
