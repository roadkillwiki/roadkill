using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public class UserException : Exception
	{
		public UserException() { }
		public UserException(string message) : base(message) { }
		public UserException(string message, Exception inner) : base(message, inner) { }
	}
}
