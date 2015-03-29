namespace Roadkill.Core.Plugins.Wysiwyg
{
    public class WysiwygButton
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
