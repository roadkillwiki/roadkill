using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Logging;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Core.Configuration
{
	/// <summary>
	/// Handles operations for the Roadkill .config file - both web.config, or custom configuration file.
	/// </summary>
	public class ConfigReaderFactory
	{
		private static ConfigReader _configReader;

		public static ConfigReader GetConfigReader(string configFilename = "")
		{
			if (_configReader == null)
			{
				_configReader = new FullTrustConfigReader(configFilename);
			}

			return _configReader;

			// TODO: Medium trust: some day in the future this may be considered:
			// - Lightspeed has perf problems with medium trust
			// - NLog doesn't work with Medium trust
			// - The plugins won't work with medium trust
			// - Repositories may not work with medium trust.
			// - Possibility that StructureMap won't work with medium trust
			// ...good luck

			//if (_configReader == null)
			//{
			//	bool isFullTrust = false;
			//	var x = AppDomain.CurrentDomain.IsFullyTrusted;
			//	if (isFullTrust)
			//	{
			//		_configReader = new FullTrustConfigReader(configFilename);
			//	}
			//	else
			//	{
			//		_configReader = new MediumTrustConfigReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roadkill.config"));
			//	}
			//}

			//return _configReader;
		}		
	}
}
