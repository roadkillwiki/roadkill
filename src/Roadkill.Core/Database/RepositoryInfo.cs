using System;

namespace Roadkill.Core.Database
{
	public class RepositoryInfo
	{
		public string Id { get; set; }
		public string Description { get; set; }

		// info == "string" (case insensitive). A null info only equals a null string, and an empty string never matches.
		public static bool operator ==(RepositoryInfo a, string b)
		{
			if (ReferenceEquals(a, null))
				return b == null;

			if (string.IsNullOrEmpty(b))
				return false;

			return string.Equals(a.Id, b, StringComparison.OrdinalIgnoreCase);
		}

		public static bool operator !=(RepositoryInfo a, string b)
		{
			return !(a == b);
		}

		// "string" == info
		public static bool operator ==(string a, RepositoryInfo b)
		{
			return b == a;
		}

		public static bool operator !=(string a, RepositoryInfo b)
		{
			return !(b == a);
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

			return string.Equals(other.Id, Id, StringComparison.OrdinalIgnoreCase);
		}

		public override int GetHashCode()
		{
			return Id == null ? 0 : StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
		}
	}
}