using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roadkill.Tests.WatinTests
{
	[TestClass]
	public class EditorTests : WatinTestBase
	{
		[TestMethod]
		public void NewPage()
		{
			using (NewSession())
			{
				Login();
			}
		}
	}
}
