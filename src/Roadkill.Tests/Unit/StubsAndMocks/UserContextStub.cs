using Roadkill.Core;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.StubsAndMocks
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
