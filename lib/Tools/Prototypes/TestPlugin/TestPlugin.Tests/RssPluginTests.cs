using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TestPlugin;

namespace ClassLibrary1
{
	[TestFixture]
	public class RssPluginTests
    {
		[Test]
		public void Test()
		{
			RssPlugin plugin = new RssPlugin();
			string html = plugin.AfterParse("[[[rss]]]");

			Console.WriteLine(html);
		}
    }
}
