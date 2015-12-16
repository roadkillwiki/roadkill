using Roadkill.Core.Configuration;
using Roadkill.Core.Security;
using Roadkill.Core.Services;

namespace Roadkill.Core.DependencyResolution
{
	/// <summary>
	/// Defines an class that has is created and has its property values setter injected by Structuremap.
	/// </summary>
	public interface ISetterInjected
	{
		ApplicationSettings ApplicationSettings { get; set; }
		IUserContext Context { get; set; }
		UserServiceBase UserService { get; set; }
		IPageService PageService { get; set; }
		SettingsService SettingsService { get; set; }
	}
}