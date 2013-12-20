using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Logging
{
	/// <summary>
	/// Manages logging in Roadkill. All logging is done via the standard TraceListeners.
	/// </summary>
	public class Log
	{
		// NLog specific
		private static Logger _logger;
		private static readonly string DEFAULT_LAYOUT = "[${longdate:universalTime=True}] [${level}] ${message}";
		private static readonly string LOGGER_NAME = "Roadkill";

		static Log()
		{
			_logger = LogManager.GetLogger("Roadkill");
		}

		/// <summary>
		/// Configures the type of logging to use based on the setting's logging types.
		/// </summary>
		public static void ConfigureLogging(ApplicationSettings settings)
		{
			if (string.IsNullOrEmpty(settings.LoggingTypes) || settings.LoggingTypes.Trim().ToLower() == "none")
			{
				LogManager.DisableLogging();
			}
			else
			{
				LogManager.EnableLogging();
				
				// Get the comma separted list of types (or it could be a single value)
				bool debugEnabled = false;
				List<string> logTypes = new List<string>();

				foreach (string value in settings.LoggingTypes.Split(','))
				{
					logTypes.Add(value.Trim().ToLower());
				}

				foreach (string type in logTypes)
				{
					switch (type)
					{
						case "textfile":
							UseTextFileLogging();
							break;

						case "logentries":
							UseLogEntriesLogging();
							break;

						case "log2console":
							UseLog2ConsoleLogging();
							break;

						case "debug":
							debugEnabled = true;
							break;

						case "all":
							UseTextFileLogging();
							UseLogEntriesLogging();
							UseLog2ConsoleLogging();
							debugEnabled = true;
							break;

						default:
							break;
					}
				}

				// Set the logging levels for all the targets
				foreach (LoggingRule rule in _logger.Factory.Configuration.LoggingRules.Where(x => x.LoggerNamePattern == LOGGER_NAME))
				{
					rule.EnableLoggingForLevel(LogLevel.Error);

					if (!settings.LogErrorsOnly)
					{
						rule.EnableLoggingForLevel(LogLevel.Info);
						rule.EnableLoggingForLevel(LogLevel.Warn);
						rule.EnableLoggingForLevel(LogLevel.Fatal);
					}

					if (debugEnabled)
					{
						rule.EnableLoggingForLevel(LogLevel.Debug);
					}
				}

				LogManager.Configuration.Reload();
			}
		}

		/// <summary>
		/// Adds ConsoleTraceListener logging to the logging listeners.
		/// </summary>
		public static void UseConsoleLogging()
		{
			ConsoleTarget target = new ConsoleTarget();
			AddNLogTarget(target, "RoadkillConsole");
		}

		/// <summary>
		/// Adds network logging.
		/// </summary>
		public static void UseLog2ConsoleLogging()
		{
			ChainsawTarget target = new ChainsawTarget();
			target.AppInfo = "Roadkill";
			target.Address = "udp://127.0.0.1:7071";

			AddNLogTarget(target, "RoadkillLog2Console");
		}

		/// <summary>
		/// Adds rolling file logging.
		/// </summary>
		public static void UseTextFileLogging()
		{
			FileTarget target = new FileTarget();
			target.FileName = @"${basedir}\App_Data\Logs\${shortdate}.log";
			target.KeepFileOpen = false;
			target.ArchiveNumbering = ArchiveNumberingMode.Rolling;

			AddNLogTarget(target, "RoadkillFile");
		}

		/// <summary>
		/// Adds TextWriterTraceListener logging to the logging listeners. The text files are written to
		/// the App_Data/Logs file as roadkill.txt and are not rolling logs.
		/// </summary>
		public static void UseLogEntriesLogging()
		{
			// See https://logentries.com/doc/dotnet/
			LogentriesTarget target = new LogentriesTarget();
			target.Key = ConfigurationManager.AppSettings["LOGENTRIES_ACCOUNT_KEY"];
			target.Location = ConfigurationManager.AppSettings["LOGENTRIES_LOCATION"];
			target.Token = ConfigurationManager.AppSettings["LOGENTRIES_TOKEN"];
			target.HttpPut = false;
			target.Ssl = false;
			target.Debug = true;

			AddNLogTarget(target, "RoadkillLogEntries");
		}

		/// <summary>
		/// Creates an information log message.
		/// </summary>
		public static void Debug(string message, params object[] args)
		{
			Write(Level.Debug, null, message, args);
		}

		/// <summary>
		/// Creates an information log message.
		/// </summary>
		public static void Information(string message, params object[] args)
		{
			Write(Level.Information, null, message, args);
		}

		/// <summary>
		/// Creates an information log message, also logging the provided exception.
		/// </summary>
		public static void Information(Exception ex, string message, params object[] args)
		{
			Write(Level.Information, ex, message, args);
		}

		/// <summary>
		/// Creates a warning log message.
		/// </summary>
		public static void Warn(string message, params object[] args)
		{
			Write(Level.Warning, null, message, args);
		}

		/// <summary>
		/// Creates a information log message, also logging the provided exception.
		/// </summary>
		public static void Warn(Exception ex, string message, params object[] args)
		{
			Write(Level.Warning, ex, message, args);
		}

		/// <summary>
		/// Creates an error (e.g. application crash) log message.
		/// </summary>
		public static void Error(string message, params object[] args)
		{
			Write(Level.Error, null, message, args);
		}

		/// <summary>
		/// Creates an error (e.g. application crash) log message, also logging the provided exception.
		/// </summary>
		public static void Error(Exception ex, string message, params object[] args)
		{
			Write(Level.Error, ex, message, args);
		}

		/// <summary>
		/// Writes a log message for the <see cref="Level"/>, and if the provided Exception is not null,
		/// appends this exception to the message.
		/// </summary>
		public static void Write(Level errorType, Exception ex, string message, params object[] args)
		{
			if (ex != null)
				message += "\n" + ex;

			switch (errorType)
			{
				case Level.Warning:
					_logger.Warn(message, args);
					break;

				case Level.Error:
					_logger.Error(message, args);
					break;

				case Level.Information:
					_logger.Info(message, args);
					break;

				case Level.Debug:
				default:
					_logger.Debug(message, args);
					break;
			}

			try
			{
				if (IsLoggingEnabled())
				{
					Console.WriteLine("[" + errorType.ToString() + "] " + message, args);
				}
			}
			catch (FormatException) { }
		}

		private static bool IsLoggingEnabled()
		{
			return _logger.Factory.IsLoggingEnabled();
		}

		/// <summary>
		/// Assigns the target to the Roadkill rule and sets its layout to [date] [level] [message]
		/// </summary>
		private static void AddNLogTarget(TargetWithLayout target, string name)
		{
			target.Layout = DEFAULT_LAYOUT;
			target.Name = name;

			if (!LogManager.Configuration.AllTargets.Contains(target))
			{
				LogManager.Configuration.AddTarget(name, target);

				LoggingRule rule = new LoggingRule(LOGGER_NAME, target);

				if (!LogManager.Configuration.LoggingRules.Contains(rule))
				{
					LogManager.Configuration.LoggingRules.Add(rule);
				}
			}
		}
	}
}
