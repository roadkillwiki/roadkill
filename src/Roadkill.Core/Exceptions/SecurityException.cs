using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// The exception that is thrown when a task for a user/role does not complete correctly. 
	/// </summary>
	public class SecurityException : ExceptionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SecurityException"/> class.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="inner">The inner exception.</param>
		public SecurityException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="SecurityException"/> class.
		/// </summary>
		/// <param name="inner">The inner exception.</param>
		/// <param name="message">The message as an <see cref="IFormattable"/> string.</param>
		/// <param name="args">Arguments for the message format.</param>
		public SecurityException(Exception inner, string message, params object[] args) : base(string.Format(message, args), inner) { }
	}
}
