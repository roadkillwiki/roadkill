using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Roadkill.Core.Common
{
	[XmlRoot("event", Namespace = "http://jakarta.apache.org/log4j/")]
	public class Log4jEvent
	{
		private static XmlSerializer _serializer = new XmlSerializer(typeof(Log4jEvent));

		private DateTime _timestamp;
		private string _message;

		[XmlIgnore]
		public long Id
		{
			get
			{
				return Timestamp.Ticks;
			}
		}

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
				if (Properties.FirstOrDefault(x => x.Name == "log4jmachinename") == null)
					Properties.Add(new Log4jProperty() { Name = "log4jmachinename", Value = "" });

				return Properties.First(x => x.Name == "log4jmachinename").Value;
			}
			set
			{
				if (Properties.FirstOrDefault(x => x.Name == "log4jmachinename") == null)
					Properties.Add(new Log4jProperty() { Name = "log4jmachinename", Value = value });
				else
					Properties.First(x => x.Name == "log4jmachinename").Value = value;
			}
		}

		[XmlIgnore]
		public string AppName
		{
			get
			{
				if (Properties.FirstOrDefault(x => x.Name == "log4japp") == null)
					Properties.Add(new Log4jProperty() { Name = "log4japp", Value = "" });

				return Properties.First(x => x.Name == "log4japp").Value;
			}
			set
			{
				if (Properties.FirstOrDefault(x => x.Name == "log4japp") == null)
					Properties.Add(new Log4jProperty() { Name = "log4japp", Value = value });
				else
					Properties.First(x => x.Name == "log4japp").Value = value;
			}
		}

		[XmlIgnore]
		public DateTime Timestamp
		{
			get
			{
				if (string.IsNullOrEmpty(_xmlTimeStamp))
					return DateTime.MinValue;
				else
					return ConvertFromUnixTimestamp(Double.Parse(_xmlTimeStamp));
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
		}

		public override bool Equals(object obj)
		{
			Log4jEvent other = obj as Log4jEvent;
			if (other == null)
				return false;

			return (other.Timestamp == Timestamp && other.Message == Message);
		}

		public string Serialize(string existingXml = "")
		{
			try
			{
				XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
				namespaces.Add("log4j", "http://jakarta.apache.org/log4j/");

				XmlWriterSettings settings = new XmlWriterSettings();
				settings.OmitXmlDeclaration = true;
				settings.Indent = true;

				StringBuilder builder = new StringBuilder();
				builder.Append(existingXml);

				using (StringWriter stringWriter = new StringWriter(builder))
				{
					using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, settings))
					{
						XmlSerializer serializer = new XmlSerializer(typeof(Log4jEvent));
						serializer.Serialize(xmlWriter, this, namespaces);

						return builder.ToString();
					}
				}
			}
			catch (Exception e)
			{
				return "";
			}
		}

		public static IEnumerable<Log4jEvent> Deserialize(string text)
		{
			try
			{
				// Work around for the log (deliberately for files?) missing the xml declaration
				if (!text.Contains("<?xml version") && !text.Contains("<log4j:events"))
				{
					text = "<?xml version=\"1.0\" encoding=\"utf-8\"?><log4j:events xmlns:log4j=\"http://jakarta.apache.org/log4j/\">" + text + "</log4j:events>";

				}

				XDocument doc = XDocument.Load(new StringReader(text));
				// Feed/Entry
				var entries = from item in doc.Root.Elements().Where(i => i.Name.LocalName == "event")
							  select Log4jEvent.DeserializeElement(item.ToString());


				return entries.ToList();
			}
			catch (Exception)
			{
				return new List<Log4jEvent>();
			}
		}

		public static Log4jEvent DeserializeElement(string text)
		{
			using (StringReader reader = new StringReader(text))
			{
				Log4jEvent log4jEvent = _serializer.Deserialize(reader) as Log4jEvent;

				if (log4jEvent == null)
					return new Log4jEvent() { Message = "" };
				else
					return log4jEvent;
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

		private DateTime ConvertFromUnixTimestamp(double milliseconds)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);;	
			return epoch.AddMilliseconds(milliseconds);
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
