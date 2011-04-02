using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	/// <summary>
	/// The exception that is thrown when a search related task does not complete correctly.
	/// </summary>
	public class SearchException : ExceptionBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SearchException"/> class.
		/// </summary>
		/// <param name="message">The message as an <see cref="IFormattable"/> string.</param>
		/// <param name="args">Arguments for the message format.</param>
		public SearchException(string message, params object[] args) : base(message, args) { }
	}
}
