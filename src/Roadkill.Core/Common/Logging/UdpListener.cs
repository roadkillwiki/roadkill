using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using log4net.Appender;
using log4net.Core;

namespace Roadkill.Core.Common
{
	public class UdpListener : TraceListener
	{
		private RemotingAppender _appender;

		public UdpListener()
		{
			_appender = new RemotingAppender();
			_appender.Sink = "tcp://localhost:7070/LoggingSink";
			_appender.Name = "RemotingAppender";
			_appender.Layout = new log4net.Layout.SimpleLayout();
			_appender.ActivateOptions();
		}

		public override void Write(string message)
		{
			var data = new LoggingEventData()
			{
				Message = message,
				Level = Level.Info,
				ThreadName = Thread.CurrentThread.Name,
				LoggerName = "UdpListener",
				TimeStamp = DateTime.Now
			};
			_appender.DoAppend(new LoggingEvent(data));
			_appender.Flush();
		}

		public override void WriteLine(string message)
		{
			Write(message +"\n");
		}
	}
}
