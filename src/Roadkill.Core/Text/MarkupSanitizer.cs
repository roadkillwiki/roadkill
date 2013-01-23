using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

// Take from: http://ajaxcontroltoolkit.codeplex.com

namespace Roadkill.Core.Converters
{
    /// <summary>
    /// Sanitizer class that allows tag and attributes those are in whitelist and removes
    /// other tags and attributes. This also cleans attribute values to remove vulnerable
    /// words and characters
    /// </summary>
    public class MarkupSanitizer
    {

        private string _applicationName;
        // declare an array to mark which characters are to be encoded.
        string[] encodedCharacters = new string[256];

        /// <summary>
        /// Constructor to initialize array of encoded values.
        /// </summary>
        public MarkupSanitizer()
        {
            // Intialize array
            for (int i = 0; i < 0xFF; i++)
            {
                if (i >= 0x30 && i <= 0x39 || i >= 0x41 && i <= 0x5A || i >= 0x61 && i <= 0x7A)
                {
                    encodedCharacters[i] = null;
                }
                else
                {
                    encodedCharacters[i] = i.ToString("X2");
                }
            }
        }

        /// <summary>
        /// Property to provide name of Application.
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return _applicationName;
            }
            set
            {
                _applicationName = value;
            }

        }

        /// <summary>
        /// Property that indicates that RequiresFullTrust is not necessary for this sanitizer. 
        /// </summary>
        public bool RequiresFullTrust
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// This accepts html text and white list for tags and attributes and 
        /// returns safe html text.
        /// </summary>
        /// <param name="htmlFragment">Html Content which need to sanitze.</param>
        /// <param name="elementWhiteList">Whitelist of tags.</param>
        /// <param name="attributeWhiteList">WhiteList of attributes.</param>
        /// <returns>Html text after sanitize.</returns>
        public string GetSafeHtmlFragment(string htmlFragment, Dictionary<string, string[]> elementWhiteList, Dictionary<string, string[]> attributeWhiteList)
        {
            return SanitizeHtml(htmlFragment, elementWhiteList, attributeWhiteList);
        }

        /// <summary>
        /// This method actually do the process of sanitization.
        /// </summary>
        /// <param name="htmlText">Html Content which need to sanitze.</param>
        /// <param name="elementWhiteList">Whitelist of tags.</param>
        /// <param name="attributeWhiteList">WhiteList of attributes.</param>
        /// <returns>Html text after sanitize.</returns>
        private string SanitizeHtml(string htmlText, Dictionary<string, string[]> elementWhiteList, Dictionary<string, string[]> attributeWhiteList)
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
            Dictionary<string, string[]> validHtmlTags = elementWhiteList;
            Dictionary<string, string[]> validAttributes = attributeWhiteList;
            string[] tagWhiteList = (from kv in validHtmlTags
                                     select kv.Key).ToArray();

			if (tagWhiteList.Count() > 0)
			{
				CleanNodes(allNodes, tagWhiteList);
			}

            // Filter the attributes of the remaining
            foreach (KeyValuePair<string, string[]> tag in validHtmlTags)
            {
                IEnumerable<HtmlNode> nodes = (from n in allNodes.DescendantsAndSelf()
                                               where n.Name == tag.Key
                                               select n);

                if (nodes == null) continue;

                foreach (var n in nodes)
                {
                    if (!n.HasAttributes) continue;

                    // Get all the allowed attributes for this tag
                    HtmlAttribute[] attr = n.Attributes.ToArray();
                    foreach (HtmlAttribute a in attr)
                    {
                        if (!tag.Value.Contains(a.Name))
                        {
                            a.Remove(); // Wasn't in the list
                        }
                        else
                        {
                            CleanAttributeValues(a);
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
        private void CleanAttributeValues(HtmlAttribute attribute)
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
            StringBuilder sbAttriuteValue = new StringBuilder();
            foreach (char c in attribute.Value.ToCharArray())
            {
                sbAttriuteValue.Append(EncodeCharacterToHtmlEntityEscape(c));
            }

            attribute.Value = sbAttriuteValue.ToString();

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
                hex = encodedCharacters[c];
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
