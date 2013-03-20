using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Common
{
	public enum ErrorType
	{
		/// <summary>
		/// Information error message, a debug message.
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
