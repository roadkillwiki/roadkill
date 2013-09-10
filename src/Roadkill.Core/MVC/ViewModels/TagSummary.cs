using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// Provides summary data for tag (cloud) information.
	/// </summary>
	public class TagSummary
	{
		/// <summary>
		/// The name of the tag
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The number of times the tag exists in the system.
		/// </summary>
		public int Count { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TagSummary"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public TagSummary(string name)
		{
			Name = name;
			Count = 1;
		}

		/// <summary>
		/// Overrides equals to compare the Name properties of two <see cref="TagSummary"/> instances.
		/// </summary>
		public override bool Equals(object obj)
		{
			TagSummary summary = obj as TagSummary;
			if (summary == null)
				return false;

			return summary.Name.Equals(Name);
		}

		/// <summary>
		/// Returns the <see cref="Name"/> property.
		/// </summary>
		public override string ToString()
		{
			return Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}
	}
}
