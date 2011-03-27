using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// A base exception class that provides message formatting.
	/// </summary>
	public class ExceptionBase : Exception
	{
		public ExceptionBase() { }
		public ExceptionBase(string message) : base(message) { }
		public ExceptionBase(string message, Exception inner) : base(message, inner) { }
		public ExceptionBase(string message, params string[] args) : base(string.Format(message, args)) { }
	}
}
