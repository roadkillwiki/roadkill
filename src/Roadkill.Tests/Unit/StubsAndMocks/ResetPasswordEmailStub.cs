using Roadkill.Core.Configuration;
using Roadkill.Core.Email;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	public class ResetPasswordEmailStub : ResetPasswordEmail
	{
		public bool IsSent { get; set; }
		public UserViewModel Model { get; set; }

		public ResetPasswordEmailStub(ApplicationSettings applicationSettings, SiteSettings siteSettings, IEmailClient emailClient)
			: base(applicationSettings, siteSettings, emailClient)
		{
		}

		public override void Send(UserViewModel model)
		{
			ReplaceTokens(model, "{EMAIL}");
			IsSent = true;
			Model = model;
		}
	}
}