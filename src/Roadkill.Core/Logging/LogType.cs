using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Logging
{
	public enum LogType
	{
		/// <summary>
		/// Don't log anything.
		/// </summary>
		None,
		/// <summary>
		/// Log to a rolling text file.
		/// </summary>
		TextFile,
		/// <summary>
		/// Log to a rolling XML file, using E2ETraceEvent format.
		/// </summary>
		XmlFile,
		/// <summary>
		/// Log using all types of logging outputs.
		/// </summary>
		All
	}
}
