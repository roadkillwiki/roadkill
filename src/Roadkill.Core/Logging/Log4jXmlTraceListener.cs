using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Roadkill.Core.Common
{
	public class Log4jXmlTraceListener : TraceListener
	{
		private static readonly int ROLLOVER_FILE_SIZE = 1024 * 1024;

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

		public Log4jXmlTraceListener(string fileName)
			: this(fileName, "Log4jXmlWriter")
		{
		}

		public Log4jXmlTraceListener(string fileName, string loggerName)
		{
			_loggerName = loggerName;
			_filename = GetRollingFilename(fileName);
			WriteXmlDeclaration();
		}

		public static string GetRollingFilename(string filename)
		{
			// Assume the current bin directory if it starts with .\ or no \
			if (filename.StartsWith(@".\") || !filename.StartsWith(@"\"))
				filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);

			// New log file every ~1mb
			try
			{
				FileInfo info = new FileInfo(filename);
				if (info.Exists && info.Length > ROLLOVER_FILE_SIZE)
				{
					string basename = Path.GetFileNameWithoutExtension(filename);
					string extension = Path.GetExtension(filename);

					string dir = Path.GetDirectoryName(filename);
					string[] files = Directory.GetFiles(dir, basename + ".*" + extension);
					int count = files.Length + 1;

					string fullName = string.Format("{0}{1}{2}{3}", basename, DateTime.Now.ToString(".yyyyMMdd."), count.ToString(), extension);

					return Path.Combine(dir, fullName);
				}
				else
				{
					return filename;
				}
			}
			catch (IOException)
			{
				return filename;
			}
		}

		private void WriteXmlDeclaration()
		{
			if (!File.Exists(_filename))
			{
				lock (_writerLock)
				{
					try
					{
						using (StreamWriter streamWriter = new StreamWriter(new FileStream(_filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write), Encoding.UTF8))
						{
							XmlWriterSettings settings = new XmlWriterSettings();
							settings.Indent = true;
							using (XmlWriter xmlWriter = XmlWriter.Create(streamWriter, settings))
							{
								xmlWriter.WriteStartElement("log4j", "events", "http://jakarta.apache.org/log4j/");
								xmlWriter.WriteString("\n");
								xmlWriter.WriteEndElement();
							}
						}
					}
					catch (IOException)
					{
					}
				}
			}
		}

		public override void Write(string message)
		{
			Write(message, "info");
		}

		public override void Write(string message, string category)
		{
			WriteLine(message, category);
		}

		public override void WriteLine(string message)
		{
			WriteLine(message, "info");
		}

		public override void WriteLine(string message, string category)
		{
			if (ShouldLogMessage(category))
			{
				lock (_writerLock)
				{
					try
					{
						using (StreamWriter streamWriter = new StreamWriter(new FileStream(_filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write), Encoding.UTF8))
						{
							AppendEventXml(streamWriter, message, category);
						}
					}
					catch (IOException)
					{
					}
				}
			}
		}

		private bool ShouldLogMessage(string category)
		{
			if (!string.IsNullOrEmpty(category))
				category = category.ToLower();

			if (Log.LogErrorsOnly && category == "error")
			{
				return true;
			}
			else if (!Log.LogErrorsOnly)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private void AppendEventXml(StreamWriter streamWriter, string message, string category)
		{
			int endElementLength = "</log4j:events>".Length;
			Log4jEvent logEvent = new Log4jEvent()
			{
				Logger = _loggerName,
				Level = Log4jEvent.GetLevelFromCategory(category),
				Timestamp = DateTime.Now,
				Message = message,
				MachineName = Environment.MachineName,
				AppName = Assembly.GetCallingAssembly().FullName
			};

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.OmitXmlDeclaration = true;

			// The following is a faster way to append to the XML document, but keeping it valid XML.
			// The alternative is to load the entire document into an XMLDocument which will consume
			// a lot of RAM once the log gets big.

			// Move the writer back to before the </events> elements
			if (streamWriter.BaseStream.Length > 0)
			{
				streamWriter.BaseStream.Position = streamWriter.BaseStream.Length;
				streamWriter.BaseStream.Position -= endElementLength +2; // add 2 for the extra line break.
			}
			else
			{
				// Invalid state, but try to recover
				WriteXmlDeclaration();
			}

			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add("log4j", "http://jakarta.apache.org/log4j/");
			
			using (XmlWriter xmlWriter = XmlWriter.Create(streamWriter, settings))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(Log4jEvent));
				serializer.Serialize(xmlWriter, logEvent, namespaces);
			}

			streamWriter.WriteLine("");
			streamWriter.WriteLine("</log4j:events>");
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
		{
			if (!Log.LogErrorsOnly)
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
						WriteLine(string.Format(format, args), "info");
						break;
				}
			}
			else if (Log.LogErrorsOnly && eventType == TraceEventType.Error)
			{
				WriteLine(string.Format(format, args), "error");
			}
		}
	}
}
