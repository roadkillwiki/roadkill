using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// The exception that is thrown when an IoC related task does not complete correctly.
	/// </summary>
	public class IoCException : ExceptionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="IoCException"/> class.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="inner">The inner exception.</param>
		public IoCException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="IoCException"/> class.
		/// </summary>
		/// <param name="inner">The inner exception.</param>
		/// <param name="message">The message as an <see cref="IFormattable"/> string.</param>
		/// <param name="args">Arguments for the message format.</param>
		public IoCException(Exception inner, string message, params object[] args) : base(string.Format(message, args), inner) { }
	}
}
