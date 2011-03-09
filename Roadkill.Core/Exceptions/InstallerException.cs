using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Roadkill.Core
{
	public class InstallerException : Exception
	{
		public InstallerException() { }
		public InstallerException(string message) : base(message) { }
		public InstallerException(string message, Exception inner) : base(message, inner) { }
	}
}