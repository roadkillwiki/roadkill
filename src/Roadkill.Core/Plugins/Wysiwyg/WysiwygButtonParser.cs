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
                .Where(doc => doc.Root != null && doc.Root.Name.LocalName.Equals("WysiwygButton"))
                .Select(doc => LoadButton(doc.Root))
                .Where(button => button != null)
                );
        }

        private WysiwygButton LoadButton(XElement root)
        {
            try
            {
                return new WysiwygButton
                {
                    Id = root.Attribute("id").Value,
                    Name = root.Attribute("name").ValueOrDefault(),
                    Title = root.Attribute("title").ValueOrDefault(),
                    Glyph = root.Attribute("glyph").ValueOrDefault(),
                    IWysiwygButton =
                        root.Element("script") != null
                            ? root.Element("script").Value
                            : File.ReadAllText(root.Attribute("scriptFile").Value)
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
