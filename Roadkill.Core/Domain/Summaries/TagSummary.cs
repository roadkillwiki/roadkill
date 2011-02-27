using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public class TagSummary
	{
		public string Name { get; set; }
		public int Count { get; set; }

		public TagSummary(string name)
		{
			Name = name;
			Count = 1;
		}

		public override bool Equals(object obj)
		{
			TagSummary summary = obj as TagSummary;
			if (summary == null)
				return false;

			return summary.Name.Equals(Name);
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
