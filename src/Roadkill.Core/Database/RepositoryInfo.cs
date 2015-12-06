using System;

namespace Roadkill.Core.Database
{
	public class RepositoryInfo
	{
		public string Id { get; set; }
		public string Description { get; set; }

		public static bool operator == (RepositoryInfo a, string b)
		{
			return a.Id.Equals(b, StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator ==(string a, RepositoryInfo b)
		{
			return b.Id.Equals(a, StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator !=(RepositoryInfo a, string b)
		{
			return !a.Id.Equals(b, StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator !=(string a, RepositoryInfo b)
		{
			return !b.Id.Equals(a, StringComparison.OrdinalIgnoreCase);
		}

		public RepositoryInfo()
		{
		}

		public RepositoryInfo(string id, string description)
		{
			Id = id;
			Description = description;
		}

		public override bool Equals(object obj)
		{
			RepositoryInfo other = obj as RepositoryInfo;
			if (other == null)
				return false;

			return other.Id.Equals(Id, StringComparison.OrdinalIgnoreCase);
		}
	}
}