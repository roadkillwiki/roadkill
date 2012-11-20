using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Roadkill.Tests.Acceptance
{
	[Category("Acceptance")]
	public class Settings
	{
		public static string HeadlessUrl { get; private set; }

		static Settings()
		{
			HeadlessUrl = "http://roadkill.apphb.com";
		}
	}
}
