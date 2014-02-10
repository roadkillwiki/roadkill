using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Email;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	public class ResetPasswordEmailStub : ResetPasswordEmail
	{
		public bool IsSent { get; set; }
		public UserViewModel Model { get; set; }

		public ResetPasswordEmailStub(ApplicationSettings applicationSettings, IRepository repository, IEmailClient emailClient)
			: base(applicationSettings, repository, emailClient)
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