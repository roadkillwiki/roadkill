using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Linq;
using System.IO;

namespace Roadkill.Core.Common
{
	public class UdpTraceListener : TraceListener
	{
		private static readonly string _xmlPrefix = "log4j";
		private static readonly string _xmlNamespace = "http://jakarta.apache.org/log4j/";

		// For 3.5/2 you can use a List but the class is no longer thread safe.
		//private Stack<string> _messageBuffer;
		private ConcurrentStack<string> _messageBuffer;

		private UdpClient _udpClient;
		private string _loggerName;

		public int BufferSize { get; set; }

		public override bool IsThreadSafe
		{
			get
			{
				return true;
			}
		}

		public UdpTraceListener()
			: this("localhost", 7071, "UdpTraceListener")
		{

		}

		public UdpTraceListener(string host, int port, string loggerName)
		{
			_loggerName = loggerName;
			_messageBuffer = new ConcurrentStack<string>();
			_udpClient = new UdpClient();
			_udpClient.Connect(host, port);
			BufferSize = 1;
		}

		public override void Write(string message)
		{
			Write(message, "info");
		}

		public override void WriteLine(string message)
		{
			Write(message, "info");
		}

		public override void WriteLine(string message, string category)
		{
			_messageBuffer.Push(CreateEventXml(message, category));

			if (_messageBuffer.Count >= BufferSize)
				Flush();
		}

		public override void Write(string message, string category)
		{
			_messageBuffer.Push(CreateEventXml(message, category));

			if (_messageBuffer.Count >= BufferSize)
				Flush();
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

		public override void Flush()
		{
			foreach (string xmlMessage in _messageBuffer)
			{
				byte[] payload = Encoding.UTF8.GetBytes(xmlMessage);
				_udpClient.Send(payload, payload.Length);
			}

			_messageBuffer.Clear();
		}

		protected override void Dispose(bool disposing)
		{
			Flush();
			_udpClient.Close();

			base.Dispose(disposing);
		}
	}
}
