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
		/// <summary>
		/// The setting name.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The setting value.
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// The UI type that should be used to represent the value (not currently implemented).
		/// </summary>
		public SettingFormType FormType { get; set; }
	}
}
