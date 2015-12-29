using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Repositories;
using Roadkill.Core.Email;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.StubsAndMocks
{
	public class ResetPasswordEmailStub : ResetPasswordEmail
	{
		public bool IsSent { get; set; }
		public UserViewModel Model { get; set; }

		public ResetPasswordEmailStub(ApplicationSettings applicationSettings, ISettingsRepository settingsRepository, IEmailClient emailClient)
			: base(applicationSettings, settingsRepository, emailClient)
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