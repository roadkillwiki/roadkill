using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using Roadkill.Core.Configuration;

namespace Roadkill.Core
{
	/// <summary>
	/// Deserializes and caches the custom tokens XML file, which contains a set of text replacements for the markup.
	/// </summary>
	internal class CustomTokenParser
	{
		private static IEnumerable<TextToken> _tokens;
		private static Dictionary<Regex,string> _regexReplacements;

		public static IEnumerable<TextToken> Tokens
		{
			get { return _tokens;  }
		}

		static CustomTokenParser()
		{
			_tokens = Deserialize();
			CacheRegexes();
		}

		public static string ReplaceTokens(string html)
		{
			foreach (Regex regex in _regexReplacements.Keys)
			{
				html = regex.Replace(html, _regexReplacements[regex]);
			}

			return html;
		}

		private static void CacheRegexes()
		{
			_regexReplacements = new Dictionary<Regex,string>();

			foreach (TextToken token in _tokens)
			{
				// Catch bad regexes
				try
				{
					Regex regex = new Regex(token.SearchRegex,RegexOptions.Compiled | RegexOptions.Singleline);
					_regexReplacements.Add(regex, token.HtmlReplacement);
				}
				catch (ArgumentException e)
				{
					Log.Warn(e, "There was an error in search regex for the token {0}", token.Name);
				}				
			}
		}

		private static IEnumerable<TextToken> Deserialize()
		{
			if (!File.Exists(RoadkillSettings.Current.ApplicationSettings.CustomTokensPath))
			{
				Log.Warn("The custom tokens file does not exist in {0}", RoadkillSettings.Current.ApplicationSettings.CustomTokensPath);
				return new List<TextToken>();
			}

			try
			{
				using (FileStream stream = new FileStream(RoadkillSettings.Current.ApplicationSettings.CustomTokensPath, FileMode.Open, FileAccess.Read))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<TextToken>));
					IEnumerable<TextToken> textTokens = (List<TextToken>)serializer.Deserialize(stream);

					if (textTokens == null)
						return new List<TextToken>();
					else
						return textTokens;
				}
			}
			catch (IOException e)
			{
				Log.Warn(e, "An IO error occurred loading the Custom tokens file {0}", RoadkillSettings.Current.ApplicationSettings.CustomTokensPath);
				return new List<TextToken>();
			}
			catch (FormatException e)
			{
				Log.Warn(e, "A FormatException error occurred loading the Custom tokens file {0}", RoadkillSettings.Current.ApplicationSettings.CustomTokensPath);
				return new List<TextToken>();
			}
			catch (Exception e)
			{
				Log.Warn(e, "An unhandled exception error occurred loading the Custom tokens file {0}", RoadkillSettings.Current.ApplicationSettings.CustomTokensPath);
				return new List<TextToken>();
			}
		}
	}
}
