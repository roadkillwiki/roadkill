using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Roadkill.Core.Mvc.ViewModels
{
	/// <summary>
	/// Provides data for the tag cloud.
	/// </summary>
	public class TagViewModel
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
		/// Initializes a new instance of the <see cref="TagViewModel"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public TagViewModel(string name)
		{
			Name = name;
			Count = 1;
		}

		/// <summary>
		/// Overrides equals to compare the Name properties of two <see cref="TagViewModel"/> instances.
		/// </summary>
		public override bool Equals(object obj)
		{
			TagViewModel summary = obj as TagViewModel;
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
