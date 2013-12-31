using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Configuration;
using Roadkill.Core.Services;
using Roadkill.Core.Security;
using StructureMap.Attributes;
using Roadkill.Core.Cache;

namespace Roadkill.Core.DI
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
