using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// The exception that is thrown when a task for user does not complete correctly. 
	/// </summary>
	public class UserException : ExceptionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UserException"/> class.
		/// </summary>
		/// <param name="message">The message as an <see cref="IFormattable"/> string.</param>
		/// <param name="args">Arguments for the message format.</param>
		public UserException(string message, params string[] args) : base(message,args) { }
	}
}
