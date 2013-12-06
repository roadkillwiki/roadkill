using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	public class FakeSignupEmail : SignupEmail
	{
		public bool IsSent { get; set; }
		public UserViewModel ViewModel { get; set; }

		public FakeSignupEmail(ApplicationSettings applicationSettings, SiteSettings siteSettings)
			: base(applicationSettings, siteSettings)
		{
		}
		
		public override void Send(UserViewModel model)
		{
			ReplaceTokens(model, "{EMAIL}");
			IsSent = true;
			ViewModel = model;
		}
	}

	public class FakeResetPasswordEmail : ResetPasswordEmail
	{
		public bool IsSent { get; set; }
		public UserViewModel Model { get; set; }

		public FakeResetPasswordEmail(ApplicationSettings applicationSettings, SiteSettings siteSettings)
			: base(applicationSettings, siteSettings)
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
