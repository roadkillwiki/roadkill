using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Plugins
{
	/// <summary>
	/// Represents a single setting value.
	/// </summary>
	public class SettingValue
	{
	    private string _value;
	    private SettingFormType _formType;

	    /// <summary>
		/// The setting name.
		/// </summary>
		public string Name { get; set; }

	    /// <summary>
	    /// The setting value.
	    /// </summary>
	    public string Value
	    {
	        get { return _value; }
	        set { _value = GetCleanValue(value, FormType); }
	    }

	    /// <summary>
	    /// The UI type that should be used to represent the value.
	    /// </summary>
	    public SettingFormType FormType
	    {
	        get { return _formType; }
	        set
	        {
                _formType = value;
                // Re-set Value to make sure it conforms to new type
	            Value = Value;
	        }
	    }

	    private static string GetCleanValue(string input, SettingFormType formType)
	    {
            switch (formType)
            {
                case SettingFormType.Checkbox:
                    // Checkbox should be boolean.
                    // The whole settings could be reworked as a generic class,
                    // quite easy with JSON serialization, but for now let's just see
                    // if the value provided by plugin creator is boolean, and if not -
                    // - revert to default (which was, is, and probably will be False)
                    bool o;
                    if (!bool.TryParse(input, out o))
                        o = default(bool);
                    return o.ToString();
                default:
                    return input;
            }
	    }
	}
}
