using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Roadkill.Core.Common;

namespace Roadkill.Core
{
	/// <summary>
	/// Manages logging in Roadkill. All logging is done via the standard TraceListeners.
	/// </summary>
	public class Log
	{
		public static void UseConsoleLogging()
		{
			//Trace.Listeners.Add(new ConsoleTraceListener());
		}

		public static void UseUdpLogging()
		{
			//Trace.Listeners.Add(new UdpTraceListener());
		}

		public static void UseXmlLogging()
		{
			string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs", "roadkill.xml.log");
			//Trace.Listeners.Add(new Log4jXmlTraceListener(logFile));
		}

		public static void UseTextFileLogging()
		{
			string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs", "roadkill.txt");
			//Trace.Listeners.Add(new TextWriterTraceListener(logFile, "roadkill-textfile"));
		}

		/// <summary>
		/// Creates an information log message.
		/// </summary>
		public static void Information(string message, params object[] args)
		{
			Write(ErrorType.Information, null, message, args);
		}

		/// <summary>
		/// Creates an information log message, also logging the provided exception.
		/// </summary>
		public static void Information(Exception ex, string message, params object[] args)
		{
			Write(ErrorType.Information, ex, message, args);
		}

		/// <summary>
		/// Creates a warning log message.
		/// </summary>
		public static void Warn(string message, params object[] args)
		{
			Write(ErrorType.Warning, null, message, args);
		}

		/// <summary>
		/// Creates a information log message, also logging the provided exception.
		/// </summary>
		public static void Warn(Exception ex, string message, params object[] args)
		{
			Write(ErrorType.Warning, ex, message, args);
		}

		/// <summary>
		/// Creates an error (e.g. application crash) log message.
		/// </summary>
		public static void Error(string message, params object[] args)
		{
			Write(ErrorType.Error, null, message, args);
		}

		/// <summary>
		/// Creates an error (e.g. application crash) log message, also logging the provided exception.
		/// </summary>
		public static void Error(Exception ex, string message, params object[] args)
		{
			Write(ErrorType.Error, ex, message, args);
		}

		/// <summary>
		/// Writes a log message for the <see cref="ErrorType"/>, and if the provided Exception is not null,
		/// appends this exception to the message.
		/// </summary>
		public static void Write(ErrorType errorType, Exception ex, string message, params object[] args)
		{
			if (ex != null)
				message += "\n" + ex;

#if !DEBUG
			//if (errorType == ErrorType.Warning)
			//	Trace.TraceError(message, args);

			//return;
#endif

			// Trace should catch FormatException
			switch (errorType)
			{
				case ErrorType.Information:
					Trace.TraceInformation(message, args);
					break;

				case ErrorType.Error:
					Trace.TraceError(message, args);
					break;

				case ErrorType.Warning:
				default:
					Trace.TraceWarning(message, args);
					break;
			}
		}
	}
}
