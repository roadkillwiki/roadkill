using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Configuration
{
	public class UpgradeChecker
	{
		public static bool IsUpgradeRequired(string version)
		{
			if (string.IsNullOrEmpty(version))
			{
				return true;
			}
			else
			{
				// Very basic checking for now:
				//  - 1.6 doesn't require an upgrade to 1.7
				//  - < 1.5 does require an upgrade.

				Version version16 = new Version(1,6,0,0);
				Version parsedVersion = null;
				if (Version.TryParse(version, out parsedVersion))
				{
					return (parsedVersion < version16);
				}
				else
				{
					Log.Warn("Upgrade check: invalid Version found ('{0}') in the web.config, assuming it's the same as the assembly version ({1})", version, ApplicationSettings.ProductVersion);
					return false;
				}
			}
		}
	}
}
