using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Roadkill.Core
{
	public class SaveException : ExceptionBase
	{
		public SaveException(string message, params string[] args) : base(message, args) { }
	}
}