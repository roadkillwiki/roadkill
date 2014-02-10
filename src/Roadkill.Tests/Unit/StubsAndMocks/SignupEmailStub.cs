using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Email;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	public class SignupEmailStub : SignupEmail
	{
		public bool IsSent { get; set; }
		public UserViewModel ViewModel { get; set; }

		public SignupEmailStub(ApplicationSettings applicationSettings, IRepository repository, IEmailClient emailClient)
			: base(applicationSettings, repository, emailClient)
		{
		}
		
		public override void Send(UserViewModel model)
		{
			ReplaceTokens(model, "{EMAIL}");
			IsSent = true;
			ViewModel = model;
		}
	}
}
