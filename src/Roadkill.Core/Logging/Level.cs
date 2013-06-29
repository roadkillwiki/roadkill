using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Logging
{
	/// <summary>
	/// The severity of a log message.
	/// </summary>
	public enum Level
	{
		/// <summary>
		/// A debug message, such as a sql statement.
		/// </summary>
		Debug,
		/// <summary>
		/// Information message, such as a cache hit.
		/// </summary>
		Information,
		/// <summary>
		/// A warning log message for when something has failed unexpectedly.
		/// </summary>
		Warning,
		/// <summary>
		/// An error log message, for an unexpected message.
		/// </summary>
		Error
	}
}
