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

		// TODO: tests
		/// <summary>
		/// Gets a CSS class name for the tag based on the <see cref="TagViewModel.Count"/> - the number of
		/// pages with that tag in the system.
		/// </summary>
		/// <param name="helper">The helper.</param>
		/// <param name="tag">A <see cref="TagViewModel"/>.</param>
		/// <returns>
		/// <list type="bullet">
		/// <item>1 tag: "tagcloud1"</item>
		/// <item>1-3 tags: "tagcloud2"</item>
		/// <item>3-5 tags: "tagcloud3"</item>
		/// <item>5-10 tags: "tagcloud4"</item>
		/// <item>10+ tags: "tagcloud5"</item>
		/// </list>
		/// </returns>
		public string ClassName
		{
			get
			{
				string className = "";

				if (Count > 10)
				{
					className = "tagcloud5";
				}
				else if (Count >= 5 && Count < 10)
				{
					className = "tagcloud4";
				}
				else if (Count >= 3 && Count < 5)
				{
					className = "tagcloud3";
				}
				else if (Count > 1 && Count < 3)
				{
					className = "tagcloud2";
				}
				else if (Count == 1)
				{
					className = "tagcloud1";
				}

				return className;
			}
		}

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
			TagViewModel model = obj as TagViewModel;
			if (model == null)
				return false;

			return model.Name.Equals(Name);
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
