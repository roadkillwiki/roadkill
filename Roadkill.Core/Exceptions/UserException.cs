using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public class UserException : ExceptionBase
	{
		public UserException(string message, params string[] args) : base(message,args) { }
	}
}
