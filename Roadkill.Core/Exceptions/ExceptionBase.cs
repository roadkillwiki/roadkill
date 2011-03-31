using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// A base exception class which provides additional <see cref="IFormattable"/> message formatting for the error message.
	/// </summary>
	public class ExceptionBase : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionBase"/> class.
		/// </summary>
		public ExceptionBase() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionBase"/> class.
		/// </summary>
		/// <param name="message">The exception message.</param>
		public ExceptionBase(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionBase"/> class.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="inner">The inner exception.</param>
		public ExceptionBase(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionBase"/> class.
		/// </summary>
		/// <param name="message">The message as an <see cref="IFormattable"/> string.</param>
		/// <param name="args">Arguments for the message format.</param>
		public ExceptionBase(string message, params object[] args) : base(string.Format(message, args)) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="ExceptionBase"/> class.
		/// </summary>
		/// <param name="inner">The inner exception.</param>
		/// <param name="message">The message as an <see cref="IFormattable"/> string.</param>
		/// <param name="args">Arguments for the message format.</param>
		public ExceptionBase(Exception inner, string message, params object[] args) : base(string.Format(message, args), inner) { }
	}
}
