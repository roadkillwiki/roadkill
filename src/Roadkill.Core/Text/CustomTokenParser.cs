using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using Roadkill.Core.Configuration;
using Roadkill.Core.Logging;
using Roadkill.Core.Plugins.Text.BuiltIn;

namespace Roadkill.Core
{
	/// <summary>
	/// Deserializes and caches the custom tokens XML file, which contains a set of text replacements for the markup.
	/// </summary>
	public class CustomTokenParser
	{
		private static IEnumerable<TextToken> _tokens;
		private static bool _isTokensFileCached;

		public static bool CacheTokensFile { get; set; }

		public IEnumerable<TextToken> Tokens
		{
			get { return _tokens;  }
		}

		static CustomTokenParser()
		{
			CacheTokensFile = true;
		}

		public CustomTokenParser(ApplicationSettings settings)
		{
			if (CacheTokensFile && !_isTokensFileCached)
			{
				_tokens = Deserialize(settings);
				ParseTokenRegexes();
				_isTokensFileCached = true;
			}
			else
			{
				_tokens = Deserialize(settings);
				ParseTokenRegexes();
			}
		}

		public string ReplaceTokensAfterParse(string html)
		{
			foreach (TextToken token in _tokens)
			{
				try
				{
					html = token.CachedRegex.Replace(html, token.HtmlReplacement);
				}
				catch (Exception e)
				{
					// Make sure no plugins bring down the parsing
					Log.Error(e, "There was an error in replacing the html for the token {0} - {1}", token.Name, e);
				}
			}

			return html;
		}

		private static void ParseTokenRegexes()
		{
			foreach (TextToken token in _tokens)
			{
				// Catch bad regexes
				try
				{
					Regex regex = new Regex(token.SearchRegex,RegexOptions.Compiled | RegexOptions.Singleline);
					token.CachedRegex = regex;
				}
				catch (ArgumentException e)
				{
					Log.Warn(e, "There was an error in search regex for the token {0}", token.Name);
				}				
			}
		}

		private static IEnumerable<TextToken> Deserialize(ApplicationSettings settings)
		{
			if (string.IsNullOrEmpty(settings.CustomTokensPath) || !File.Exists(settings.CustomTokensPath))
			{
				if (!string.IsNullOrEmpty(settings.CustomTokensPath))
					Log.Warn("The custom tokens file does not exist in path '{0}' - using an empty token list.", settings.CustomTokensPath);

				return new List<TextToken>();
			}

			try
			{
				using (FileStream stream = new FileStream(settings.CustomTokensPath, FileMode.Open, FileAccess.Read))
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
				Log.Warn(e, "An IO error occurred loading the custom tokens file {0}", settings.CustomTokensPath);
				return new List<TextToken>();
			}
			catch (FormatException e)
			{
				Log.Warn(e, "A FormatException error occurred loading the custom tokens file {0}", settings.CustomTokensPath);
				return new List<TextToken>();
			}
			catch (InvalidOperationException e)
			{
				Log.Warn(e, "An InvalidOperationException (bad XML file) error occurred loading the custom tokens file {0}", settings.CustomTokensPath);
				return new List<TextToken>();
			}
		}
	}
}
