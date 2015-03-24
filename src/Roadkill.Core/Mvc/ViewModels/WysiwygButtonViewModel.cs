using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roadkill.Core.Mvc.ViewModels
{
    /// <summary>
    /// Represents a single button that's displayed on Wysiwyg toolbar
    /// </summary>
    public class WysiwygButtonViewModel
    {
        /// <summary>
        /// Id used for JS-hookups - will be added as a class to button element
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Title - tooltip
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Displayed inside the button
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Glyph from Bootstrap glyphicons or leave null/empty to not display any icon
        /// </summary>
        public string Glyph { get; set; }

        /// <summary>
        /// String containing JS object that has to conform to IWysiwygButton TS interface
        /// </summary>
        public string IWysiwygButton { get; set; }
    }
}
