using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Roadkill.Core.Logging;

namespace Roadkill.Core.Configuration
{
	internal class ConfigFileSerializer
	{
		public static string Serialize<T>(T obj) where T : class
		{
			StringBuilder builder = new StringBuilder();
			using (StringWriter writer = new StringWriter(builder))
			{
				XmlSerializer serializer = new XmlSerializer(typeof(T));
				serializer.Serialize(writer, obj);

				return builder.ToString();
			}
		}

		public static T SafeDeserialize<T>(string filePath) where T : class
		{
			try
			{
				using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(T));
					T result = serializer.Deserialize(stream) as T;

					if (result == null)
						return default(T);
					else
						return result;
				}
			}
			catch (IOException e)
			{
				Log.Warn(e, "An IO error occurred de-serializing the XML config file {0}", filePath);
				return default(T);
			}
			catch (FormatException e)
			{
				Log.Warn(e, "A FormatException error occurred de-serializing XML config file {0}", filePath);
				return default(T);
			}
			catch (InvalidOperationException e)
			{
				Log.Warn(e, "An InvalidOperationException (bad XML file) error occurred de-serializing XML config file {0}", filePath);
				return default(T);
			}
		}

		public static string JsonSerialize<T>(T obj)
		{
			JsonConvert.DefaultSettings = (() =>
			{
				// Turn enums from numbers into names
				var settings = new JsonSerializerSettings();
				settings.Converters.Add(new StringEnumConverter { CamelCaseText = false });
				settings.Formatting = Newtonsoft.Json.Formatting.Indented;
				return settings;
			});

			return JsonConvert.SerializeObject(obj);
		}

		public static T JsonDeserialize<T>(string json)
		{
			T obj = (T)JsonConvert.DeserializeObject(json, typeof(T));
			return obj;

			// TODO recover
		}
	}
}
