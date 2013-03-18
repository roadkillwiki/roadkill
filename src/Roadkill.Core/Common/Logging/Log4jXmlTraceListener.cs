using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Common
{
	public class Log4jXmlTraceListener : TraceListener
	{
		private string _loggerName;
		private string _filename;
		private object _writerLock = new object();

		public override bool IsThreadSafe
		{
			get
			{
				return true;
			}
		}

		public Log4jXmlTraceListener(string fileName) : this(fileName, "Log4jXmlWriter")
		{
		}

		public Log4jXmlTraceListener(string fileName, string loggerName)
		{
			_loggerName = loggerName;
			_filename = fileName;

			// Assume the current bin directory if it starts with .\ or no \
			if (_filename.StartsWith(@".\") || !_filename.StartsWith(@"\"))
				_filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _filename);
		}

		public override void Write(string message)
		{
			Write(message, "info");
		}

		public override void Write(string message, string category)
		{
			lock (_writerLock)
			{
				using (StreamWriter streamWriter = new StreamWriter(new FileStream(_filename, FileMode.Append, FileAccess.Write, FileShare.Write), Encoding.UTF8))
				{
					streamWriter.WriteLine(CreateEventXml(message, category));
				}
			}
		}

		public override void WriteLine(string message)
		{
			WriteLine(message, "info");
		}

		public override void WriteLine(string message, string category)
		{
			lock (_writerLock)
			{
				using (StreamWriter streamWriter = new StreamWriter(new FileStream(_filename, FileMode.Append, FileAccess.Write, FileShare.Write), Encoding.UTF8))
				{
					streamWriter.WriteLine(CreateEventXml(message, category));
				}
			}
		}

		private string CreateEventXml(string message, string category)
		{
			Log4jEvent log = new Log4jEvent()
			{
				Logger = _loggerName,
				Level = Log4jEvent.GetLevelFromCategory(category),
				Timestamp = DateTime.Now,
				Message = message,
			};

			return log.Serialize();
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			switch (eventType)
			{
				case TraceEventType.Critical:
				case TraceEventType.Error:
					WriteLine(string.Format(format, args), "error");
					break;

				case TraceEventType.Verbose:
					WriteLine(string.Format(format, args), "debug");
					break;

				case TraceEventType.Warning:
					WriteLine(string.Format(format, args), "warn");
					break;

				case TraceEventType.Information:
				case TraceEventType.Resume:
				case TraceEventType.Start:
				case TraceEventType.Stop:
				case TraceEventType.Suspend:
				case TraceEventType.Transfer:
				default:
					WriteLine(string.Format(format, args), "error");
					break;
			}
		}
	}
}
