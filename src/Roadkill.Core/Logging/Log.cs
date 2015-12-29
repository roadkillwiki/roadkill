using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;
using Roadkill.Core.Configuration;

namespace Roadkill.Core.Logging
{
	/// <summary>
	/// Manages logging in Roadkill.
	/// </summary>
	public class Log
	{
		private static readonly Logger _logger;
		private static readonly string LOGGER_NAME = "Roadkill";

		public static string NLogConfigPath { get; set; }

		static Log()
		{
			_logger = LogManager.GetLogger("Roadkill");
		}

		/// <summary>
		/// Configures the log files used, this should be called on application startup.
		/// </summary>
		/// <param name="settings">Used to get the path of the NLog.config file</param>
		public static void ConfigureLogging(ApplicationSettings settings)
		{
			if (string.IsNullOrEmpty(settings.NLogConfigFilePath))
				throw new ConfigurationException(null, "The NLog.config path is null/empty (ApplicationSettings.NLogConfigFilePath).");

			string path = settings.NLogConfigFilePath;
			path = path.Replace('/', Path.DirectorySeparatorChar);

			if (path.StartsWith("~"))
			{
				path = path.Replace("~", AppDomain.CurrentDomain.BaseDirectory);
			}

			if (!File.Exists(path))
			{
				throw new ConfigurationException(null, "The NLog.config path does not exist: {0}", path);
			}
			
			NLogConfigPath = path;
			LogManager.Configuration = new XmlLoggingConfiguration(NLogConfigPath, true);
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
		}
	}
}
