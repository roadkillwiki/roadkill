using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Explicit as this test cannot run with using a headless browser.
	/// </summary>
	public class RequiresBrowserWithJavascriptAttribute : Attribute //: ExplicitAttribute
	{
	}
}
