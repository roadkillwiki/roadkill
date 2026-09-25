namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// The SMTP settings for sending emails (previously the system.net/mailSettings section of the web.config).
	/// </summary>
	public class SmtpSettings
	{
		public string Host { get; set; } = "";
		public int Port { get; set; } = 25;
		public string Username { get; set; } = "";
		public string Password { get; set; } = "";
		public bool EnableSsl { get; set; }
		public string From { get; set; } = "signup@roadkillwiki.net";
		public string PickupDirectory { get; set; } = "~/App_Data/TempSmtp";
	}
}
