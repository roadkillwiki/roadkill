using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Core.Security.Permissions.Capabilities
{
	// These values are NOT bitwise - one capability doesn't inherit the others below it.

	public enum IOCapability
	{
		View,
		Upload,
		DeleteFile,
		DeleteFolder 
	}
}