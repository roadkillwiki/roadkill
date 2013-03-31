using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Essential.Diagnostics;
using Roadkill.Core.Common;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Manages logging in Roadkill. All logging is done via the standard TraceListeners.
	/// </summary>
	public class Log
	{
		public static bool LogErrorsOnly { get; set; }

		/// <summary>
		/// Configures the type of log file to use based on the configuration, and 
		/// whether to all messages or just errors.
		/// </summary>
		/// <param name="settings"></param>
		public static void ConfigureLogging(ApplicationSettings settings)
		{
			LogErrorsOnly = settings.LogErrorsOnly;

			switch (settings.LoggingType)
			{
				case LogType.None:
					break;

				case LogType.TextFile:
					UseTextFileLogging();
					break;

				case LogType.XmlFile:
					UseXmlLogging();
					break;

				case LogType.All:
					UseTextFileLogging();
					UseXmlLogging();
					break;

				default:
					break;
			}

#if DEBUG
			UseUdpLogging();
#endif
		}

		/// <summary>
		/// Adds ConsoleTraceListener logging to the logging listeners.
		/// </summary>
		public static void UseConsoleLogging()
		{
			Trace.Listeners.Add(new ConsoleTraceListener());
		}

		/// <summary>
		/// Adds UdpTraceListener logging to the logging listeners.
		/// </summary>
		public static void UseUdpLogging()
		{
			Trace.Listeners.Add(new UdpTraceListener());
		}

		/// <summary>
		/// Adds Log4jXmlTraceListener (log4j format XML file logging) to the logging listeners.
		/// The XML files are written to the App_Data/Logs file as roadkill.xml.log and a new file is created 
		/// when the log file reaches 1mb.
		/// </summary>
		public static void UseXmlLogging()
		{
			string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs", "roadkill.log-{DateTime:yyyy-MM-dd}.xml");
			Trace.Listeners.Add(new RollingXmlTraceListener(logFile));
		}

		/// <summary>
		/// Adds TextWriterTraceListener logging to the logging listeners. The text files are written to
		/// the App_Data/Logs file as roadkill.txt and are not rolling logs.
		/// </summary>
		public static void UseTextFileLogging()
		{
			string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "Logs", "roadkill-{DateTime:yyyy-MM-dd}.log");
			Trace.Listeners.Add(new RollingFileTraceListener(logFile));
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

			if (LogErrorsOnly && errorType == ErrorType.Warning)
			{
				Trace.TraceError(message, args);
			}
			else
			{
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
}
