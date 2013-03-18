using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace Roadkill.Core.Common
{
	[XmlRoot("event", Namespace = "http://jakarta.apache.org/log4j/")]
	public class Log4jEvent
	{
		private DateTime _timestamp;
		private string _message;

		[XmlAttribute("timestamp")]
		public string _xmlTimeStamp;

		[XmlAttribute("logger")]
		public string Logger { get; set; }

		[XmlAttribute("level")]
		public string Level { get; set; }

		[XmlAttribute("thread")]
		public string Thread { get; set; }

		[XmlElement("message")]
		public XmlCDataSection CDataMessage
		{
			get
			{
				XmlDocument doc = new XmlDocument();
				return doc.CreateCDataSection(RemoveInvalidXmlChars(_message));
			}
			set
			{
				_message = value.Value;
			}
		}

		[XmlElement("throwable")]
		public string Exception { get; set; }

		[XmlArray("properties")]
		public List<Log4jProperty> Properties { get; private set; }

		[XmlIgnore]
		public string MachineName
		{
			get
			{
				return Properties.First(x => x.Name == "log4jmachinename").Value;
			}
			set
			{
				Properties.First(x => x.Name == "log4jmachinename").Value = value;
			}
		}

		[XmlIgnore]
		public string AppName
		{
			get
			{
				return Properties.First(x => x.Name == "log4jmachinename").Value;
			}
			set
			{
				Properties.First(x => x.Name == "log4jmachinename").Value = value;
			}
		}

		[XmlIgnore]
		public DateTime Timestamp
		{
			get
			{
				return _timestamp;
			}
			set
			{
				_timestamp = value;
				_xmlTimeStamp = XmlConvert.ToString(ConvertToUnixTimestamp(value));
			}
		}

		[XmlIgnore]
		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				_message = value;
			}
		}

		public Log4jEvent()
		{
			Properties = new List<Log4jProperty>();
			Properties.Add(new Log4jProperty() { Name = "log4jmachinename", Value = Environment.MachineName });
			Properties.Add(new Log4jProperty() { Name = "log4japp", Value = Assembly.GetCallingAssembly().FullName });
		}

		public string Serialize()
		{
			XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
			namespaces.Add("log4j", "http://jakarta.apache.org/log4j/");

			XmlWriterSettings settings = new XmlWriterSettings();
			settings.OmitXmlDeclaration = true;
			settings.Indent = true;

			StringBuilder builder = new StringBuilder();

			using (StringWriter writer = new StringWriter(builder))
			{
				using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(Log4jEvent));
					serializer.Serialize(xmlWriter, this, namespaces);

					return builder.ToString();
				}
			}
		}

		public static string GetLevelFromCategory(string category)
		{
			string level = "INFO";
			if (string.IsNullOrEmpty(category))
				category = "info";

			switch (category.ToLower())
			{
				case "fatal":
					level = "FATAL";
					break;

				case "warning":
				case "warn":
					level = "WARN";
					break;

				case "error":
					level = "ERROR";
					break;

				case "debug":
					level = "DEBUG";
					break;

				case "trace":
					level = "TRACE";
					break;

				default:
					break;
			}

			return level;
		}

		private double ConvertToUnixTimestamp(DateTime date)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			TimeSpan sinceEpoch = date.ToUniversalTime() - epoch;
			return Math.Floor(sinceEpoch.TotalMilliseconds);
		}

		private string RemoveInvalidXmlChars(string text)
		{
			var validXmlChars = text.Where(x => XmlConvert.IsXmlChar(x)).ToArray();
			return new string(validXmlChars);
		}

		[XmlType("data", Namespace = "http://jakarta.apache.org/log4j/")]
		public class Log4jProperty
		{
			[XmlAttribute("name")]
			public string Name { get; set; }

			[XmlAttribute("value")]
			public string Value { get; set; }
		}
	}
}
