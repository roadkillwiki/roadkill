using System;

namespace Roadkill.Core.Exceptions
{
	class FileException : ExceptionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FileException"/> class.
		/// </summary>
		/// <param name="message">The exception message.</param>
		/// <param name="inner">The inner exception.</param>
		public FileException(string message, Exception inner) : base(message, inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FileException"/> class.
		/// </summary>
		/// <param name="inner">The inner exception.</param>
		/// <param name="message">The message as an <see cref="IFormattable"/> string.</param>
		/// <param name="args">Arguments for the message format.</param>
		public FileException(Exception inner, string message, params object[] args) : base(string.Format(message, args), inner) { }
	}
}
