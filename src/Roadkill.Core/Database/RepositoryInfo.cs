using System;

namespace Roadkill.Core.Database
{
	public class RepositoryInfo
	{
		public string Id { get; set; }
		public string Description { get; set; }

		// info == "string"
		public static bool operator == (RepositoryInfo a, string b)
		{
			if (ReferenceEquals(a, null))
				return ReferenceEquals(a, null);

			if (string.IsNullOrEmpty(b))
				return false;

			return a.Id.Equals(b, StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator !=(RepositoryInfo a, string b)
		{
			if (ReferenceEquals(a, null))
				return ReferenceEquals(a, null);

			if (string.IsNullOrEmpty(b))
				return true;

			return !a.Id.Equals(b, StringComparison.OrdinalIgnoreCase);
		}

		// "string" == info
		public static bool operator ==(string a, RepositoryInfo b)
		{
			if (ReferenceEquals(b, null))
				return ReferenceEquals(b, null);

			if (string.IsNullOrEmpty(a))
				return false;

			return b.Id.Equals(a, StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator !=(string a, RepositoryInfo b)
		{
			if (ReferenceEquals(b, null))
				return ReferenceEquals(b, null);

			if (string.IsNullOrEmpty(a))
				return true;

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