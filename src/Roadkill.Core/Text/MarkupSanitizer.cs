using System.Collections.Generic;
using HtmlAgilityPack;
using System.Text;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

// Taken from: http://ajaxcontroltoolkit.codeplex.com

namespace Roadkill.Core.Converters
{
	public enum SanitizeMode
	{
		Custom,
		Loose,
		Strict
	}

    /// <summary>
    /// Sanitizer class that allows tag and attributes those are in whitelist and removes
    /// other tags and attributes. This also cleans attribute values to remove vulnerable
    /// words and characters
    /// </summary>
    public class MarkupSanitizer
    {
		// declare an array to mark which characters are to be encoded.
		private string[] _encodedCharacters = new string[256];

		public SanitizeMode Mode { get; private set; }
		public Dictionary<string, string[]> ElementWhiteList { get; private set; }
		public Dictionary<string, string[]> AttributeWhiteList { get; private set; }

		public MarkupSanitizer() 
			: this(SanitizeMode.Strict, null, null)
		{
		}

		public MarkupSanitizer(SanitizeMode mode)
			: this(mode, null, null)
		{
		}

        /// <summary>
        /// Constructor to initialize array of encoded values.
        /// </summary>
        public MarkupSanitizer(SanitizeMode mode, Dictionary<string, string[]> elementWhiteList, Dictionary<string, string[]> attributeWhieList)
        {
            // Intialize array
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

			if (elementWhiteList != null)
			{
				ElementWhiteList = elementWhiteList;
				Mode = SanitizeMode.Custom;
			}
			else
			{
				Mode = mode;
				ElementWhiteList = CreateElementWhiteList();

				if (mode == SanitizeMode.Loose)
				{

				}
			}

			if (attributeWhieList != null)
			{
				AttributeWhiteList = attributeWhieList;
			}
			else
			{
				Mode = mode;
				AttributeWhiteList = CreateAttributeWhiteList();

				if (mode == SanitizeMode.Loose)
				{

				}
			}
        }

		private Dictionary<string, string[]> CreateElementWhiteList()
		{
			// make list of tags and its relatd attributes
			Dictionary<string, string[]> tagList = new Dictionary<string, string[]>();

			tagList.Add("strong", new string[] { "style", });
			tagList.Add("b", new string[] { "style" });
			tagList.Add("em", new string[] { "style" });
			tagList.Add("i", new string[] { "style" });
			tagList.Add("u", new string[] { "style" });
			tagList.Add("strike", new string[] { "style" });
			tagList.Add("sub", new string[] { });
			tagList.Add("sup", new string[] { });
			tagList.Add("p", new string[] { "style", "align", "dir" });
			tagList.Add("ol", new string[] { });
			tagList.Add("li", new string[] { });
			tagList.Add("ul", new string[] { });
			tagList.Add("font", new string[] { "style", "color", "face", "size" });
			tagList.Add("blockquote", new string[] { "style", "dir" });
			tagList.Add("hr", new string[] { "size", "width" });
			tagList.Add("img", new string[] { "src" });
			tagList.Add("div", new string[] { "style", "align" });
			tagList.Add("span", new string[] { "style" });
			tagList.Add("br", new string[] { "style" });
			tagList.Add("center", new string[] { "style" });
			tagList.Add("a", new string[] { "href" });
			tagList.Add("pre", new string[] { "id" });
			tagList.Add("code", new string[] { "id" });

			return tagList;
		}

		private Dictionary<string, string[]> CreateAttributeWhiteList()
		{
			Dictionary<string, string[]> attributeList = new Dictionary<string, string[]>();

			// create white list of attributes and its values
			attributeList.Add("style", new string[] { "background-color", "margin", "margin-right", "padding", "border", "text-align" });
			attributeList.Add("align", new string[] { "left", "right", "center", "justify" });
			attributeList.Add("color", new string[] { });
			attributeList.Add("size", new string[] { });
			attributeList.Add("face", new string[] { });
			attributeList.Add("dir", new string[] { "ltr", "rtl", "Auto" });
			attributeList.Add("width", new string[] { });
			attributeList.Add("src", new string[] { });
			attributeList.Add("href", new string[] { });

			return attributeList;
		}

        /// <summary>
        /// This method actually do the process of sanitization.
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
            Dictionary<string, string[]> validHtmlTags = ElementWhiteList;
            Dictionary<string, string[]> validAttributes = AttributeWhiteList;
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
