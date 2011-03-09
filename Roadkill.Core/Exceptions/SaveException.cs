using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Roadkill.Core
{
	public class SaveException : Exception
	{
		public SaveException() { }
		public SaveException(string message) : base(message) { }
		public SaveException(string message, Exception inner) : base(message, inner) { }
	}
}