using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Core
{
	public class Salt
	{
		private static Random _random = new Random();
		public string Value { get; set; }

		public Salt()
		{
			StringBuilder builder = new StringBuilder(16);
			for (int i = 0; i < 16; i++)
			{
				builder.Append((char)_random.Next(33, 126));
			}

			Value = builder.ToString();
		}

		public static implicit operator string(Salt salt)
		{
			return salt.Value;
		}

		public override string ToString()
		{
			return Value;
		}
	}
}
