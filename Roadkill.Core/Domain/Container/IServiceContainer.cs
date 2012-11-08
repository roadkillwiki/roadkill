using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Search;

namespace Roadkill.Core.Domain
{
	public interface IServiceContainer
	{
		UserManager UserManager { get; }
		PageManager PageManager { get; }
		SearchManager SearchManager { get; }
		SettingsManager SettingsManager { get; }
		HistoryManager HistoryManager { get; }
		IRepository Repository { get; }
		IConfigurationContainer Configuration { get; }
	}
}