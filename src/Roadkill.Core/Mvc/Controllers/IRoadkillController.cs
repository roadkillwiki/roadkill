using Roadkill.Core.Configuration;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.Mvc.Controllers
{
	public interface IRoadkillController
	{
		ApplicationSettings ApplicationSettings { get; }
		UserServiceBase UserService { get; }
		IUserContext Context { get; }
		SettingsService SettingsService { get; }
	}
}