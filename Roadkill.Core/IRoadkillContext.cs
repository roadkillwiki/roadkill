using System;

namespace Roadkill.Core
{
	public interface IRoadkillContext
	{
		string CurrentUser { get; set; }
		string CurrentUsername { get; }
		bool IsAdmin { get; }
		bool IsContentPage { get; }
		bool IsEditor { get; }
		bool IsLoggedIn { get; }
		PageSummary Page { get; set; }
	}
}
