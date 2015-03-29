using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NLog;
using Roadkill.Core.Configuration;
using Roadkill.Core.Extensions;

namespace Roadkill.Core.Plugins.Wysiwyg
{
    public class WysiwygButtonParser
    {
        private readonly List<WysiwygButton> _buttons = new List<WysiwygButton>();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public IReadOnlyCollection<WysiwygButton> Buttons
        {
            get { return _buttons; }
        }

        public WysiwygButtonParser(ApplicationSettings settings)
        {
            var wButtonDir = new DirectoryInfo(settings.WysiwygButtonsPath);
            if(wButtonDir.Exists)
                Load(wButtonDir);
        }

        private void Load(DirectoryInfo wButtonDir)
        {
            _buttons.Clear();
            _buttons.AddRange(wButtonDir.EnumerateFiles("*.xml")
                .Select(fi => XDocument.Load(fi.FullName))
                .Where(doc => doc.Root != null)
                .SelectMany(doc => doc.Root.Elements("WysiwygButton").Select(LoadButton))
                .Where(button => button != null)
                );
        }

        private WysiwygButton LoadButton(XElement element)
        {
            try
            {
                return new WysiwygButton
                {
                    Id = element.Attribute("id").Value,
                    Name = element.Attribute("name").ValueOrDefault(),
                    Title = element.Attribute("title").ValueOrDefault(),
                    Glyph = element.Attribute("glyph").ValueOrDefault(),
                    IWysiwygButton =
                        element.Element("script") != null
                            ? element.Element("script").Value
                            : File.ReadAllText(element.Attribute("scriptFile").Value)
                };
            }
            catch (Exception e)
            {
                Log.ErrorException("Error while loading WysiwygButton from xml", e);
                return null;
            }
        }
    }
}
