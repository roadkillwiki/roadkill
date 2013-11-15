using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	internal class UserContextStub : IUserContext
	{
		public string CurrentUser { get; set; }
		public string CurrentUsername { get; set; }
		public bool IsAdmin { get; set; }
		public bool IsContentPage { get; set; }
		public bool IsEditor { get; set; }
		public bool IsLoggedIn { get; set; }
		public PageViewModel Page { get; set; }
	}
}
