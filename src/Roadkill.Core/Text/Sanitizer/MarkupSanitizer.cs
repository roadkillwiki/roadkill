using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using HtmlAgilityPack;
using Roadkill.Core.Configuration;
using HapHtmlAttribute = HtmlAgilityPack.HtmlAttribute;

// Parts of this class are based on source (c) 2009 Codeplex Foundation 
// from: http://ajaxcontroltoolkit.codeplex.com under the new BSD license.
namespace Roadkill.Core.Text.Sanitizer
{
    /// <summary>
    /// Sanitizer class that allows tag and attributes those are in whitelist and removes
    /// other tags and attributes. This also cleans attribute values to remove vulnerable
    /// words and characters
    /// </summary>
    public class MarkupSanitizer
    {
		private string[] _encodedCharacters = new string[256];
		private ApplicationSettings _applicationSettings;
		private string _cacheKey;
		internal static MemoryCache _memoryCache = new MemoryCache("MarkupSanitizer");

		/// <summary>
		/// 
		/// </summary>
		/// <param name="settings"></param>
		public MarkupSanitizer(ApplicationSettings settings) 
		{
			_applicationSettings = settings;
			_cacheKey = "whitelist";

			// Intialize an array to mark which characters are to be encoded.
            for (int i = 0; i < 0xFF; i++)
            {
                if (i >= 0x30 && i <= 0x39 || i >= 0x41 && i <= 0x5A || i >= 0x61 && i <= 0x7A)
                {
                    _encodedCharacters[i] = null;
                }
                else
                {
                    _encodedCharacters[i] = i.ToString("X2");
                }
            }
        }

		/// <summary>
		/// Changes the key name used for the cache'd version of the HtmlWhiteList object.
		/// </summary>
		/// <param name="key"></param>
		public void SetWhiteListCacheKey(string key)
		{
			_memoryCache.Remove(_cacheKey);
			_cacheKey = key;
		}

		/// <summary>
		/// A MemoryCache is used as an alternative to a unit-test unfriendly static HtmlWhiteList.
		/// </summary>
		private HtmlWhiteList GetCachedWhiteList()
		{
			HtmlWhiteList whiteList = _memoryCache.Get(_cacheKey) as HtmlWhiteList;

			if (whiteList == null)
			{
				whiteList = HtmlWhiteList.Deserialize(_applicationSettings);
				_memoryCache.Add(_cacheKey, whiteList, new CacheItemPolicy());
			}

			return whiteList;
		}

        /// <summary>
        /// Removes all tags from a html string that aren't in the whitelist.
        /// </summary>
        /// <param name="htmlText">Html Content which need to sanitze.</param>
        /// <returns>Html text after sanitize.</returns>
        public string SanitizeHtml(string htmlText)
        {
            // Create Html document
            HtmlDocument html = new HtmlDocument();
            html.OptionFixNestedTags = true;
            html.OptionAutoCloseOnEnd = true;
            html.OptionDefaultStreamEncoding = Encoding.UTF8;
            html.LoadHtml(htmlText);

            if (html == null)
                return string.Empty;

            HtmlNode allNodes = html.DocumentNode;
			string[] tagNames = GetCachedWhiteList().ElementWhiteList.Select(x => x.Name).ToArray();
			CleanNodes(allNodes, tagNames);

            // Filter the attributes of the remaining
			foreach (HtmlElement whiteListTag in GetCachedWhiteList().ElementWhiteList)
            {
                IEnumerable<HtmlNode> nodes = (from n in allNodes.DescendantsAndSelf()
                                               where n.Name == whiteListTag.Name
                                               select n);

                if (nodes == null) continue;

                foreach (HtmlNode node in nodes)
                {
                    if (!node.HasAttributes) continue;

                    // Get all the allowed attributes for this tag
                    HapHtmlAttribute[] attributes = node.Attributes.ToArray();
					foreach (HapHtmlAttribute attribute in attributes)
                    {
                        if (!whiteListTag.ContainsAttribute(attribute.Name))
                        {
                            attribute.Remove(); // Wasn't in the list
                        }
                        else
                        {
                            CleanAttributeValues(attribute);
                        }
                    }
                }
            }

            return allNodes.InnerHtml;
        }

        /// <summary>
        /// This removes the current node tags and its child nodes if these are not in whitelist.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="tagWhiteList"></param>
        private void CleanNodes(HtmlNode node, string[] tagWhiteList)
        {
            // remove node that is not in the whitelist.
            if (node.NodeType == HtmlNodeType.Element)
            {
                if (!tagWhiteList.Contains(node.Name))
                {
                    node.ParentNode.RemoveChild(node);
                    return; // We're done
                }
            }

            // remove nested nodes those are not in the whitelist.
            if (node.HasChildNodes)
                CleanChildren(node, tagWhiteList);
        }

        /// <summary>
        /// Apply CleanNodes to each of the child nodes
        /// </summary>
        private void CleanChildren(HtmlNode parent, string[] tagWhiteList)
        {
            for (int i = parent.ChildNodes.Count - 1; i >= 0; i--)
                CleanNodes(parent.ChildNodes[i], tagWhiteList);
        }

        /// <summary>
        /// This removes the vulnerable keywords and make values safe by html encoding and html character escaping.
        /// </summary>        
        /// <param name="attribute">Attribute that contain values that need to check and clean.</param>
        private void CleanAttributeValues(HapHtmlAttribute attribute)
        {
            attribute.Value = HttpUtility.HtmlEncode(attribute.Value);
            
            attribute.Value = Regex.Replace(attribute.Value, @"\s*j\s*a\s*v\s*a\s*s\s*c\s*r\s*i\s*p\s*t\s*", "", RegexOptions.IgnoreCase);
            attribute.Value = Regex.Replace(attribute.Value, @"\s*s\s*c\s*r\s*i\s*p\s*t\s*", "", RegexOptions.IgnoreCase);

            if (attribute.Name.ToLower() == "style")
            {
                attribute.Value = Regex.Replace(attribute.Value, @"\s*e\s*x\s*p\s*r\s*e\s*s\s*s\s*i\s*o\s*n\s*", "", RegexOptions.IgnoreCase);
                attribute.Value = Regex.Replace(attribute.Value, @"\s*b\s*e\s*h\s*a\s*v\s*i\s*o\s*r\s*", "", RegexOptions.IgnoreCase);             
            }

            if (attribute.Name.ToLower() == "href" || attribute.Name.ToLower() == "src")
            {
                //if (!attribute.Value.StartsWith("http://") || attribute.Value.StartsWith("/"))
                //    attribute.Value = "";
                attribute.Value = Regex.Replace(attribute.Value, @"\s*m\s*o\s*c\s*h\s*a\s*", "", RegexOptions.IgnoreCase);
            }

            // HtmlEntity Escape
            StringBuilder sbAttributeValue = new StringBuilder();
            foreach (char c in attribute.Value.ToCharArray())
            {
                sbAttributeValue.Append(EncodeCharacterToHtmlEntityEscape(c));
            }

            attribute.Value = sbAttributeValue.ToString();

        }

        /// <summary>
        /// To encode html attribute characters to hex format except alphanumeric characters. 
        /// </summary>
        /// <param name="c">Character from the attribute value</param>
        /// <returns>Hex formatted string.</returns>
        private string EncodeCharacterToHtmlEntityEscape(char c)
        {   
            string hex; 
            // check for alphnumeric characters
            if (c < 0xFF)
            {
                hex = _encodedCharacters[c];
                if (hex == null)
                    return "" + c;
            }
            else
                hex = ((int)(c)).ToString("X2");
            
            // check for illegal characters
            if ((c <= 0x1f && c != '\t' && c != '\n' && c != '\r') || (c >= 0x7f && c <= 0x9f))
            {
                hex = "fffd"; // Let's entity encode this instead of returning it
            }

            return "&#x" + hex + ";";
        }
    }
}
