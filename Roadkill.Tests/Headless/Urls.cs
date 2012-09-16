using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roadkill.Tests.Headless
{
	public class Settings
	{
		public static string HeadlessUrl { get; private set; }

		static Settings()
		{
			HeadlessUrl = "http://roadkill.apphb.com";
		}
	}
}
