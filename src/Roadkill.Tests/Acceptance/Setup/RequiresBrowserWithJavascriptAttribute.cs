using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Roadkill.Tests.Acceptance
{
	/// <summary>
	/// Marks a test (for future use) as not being able to be run in a headless browser.
	/// </summary>
	public class RequiresBrowserWithJavascriptAttribute : TestAttribute // ExplicitAttribute
	{
		
	}
}
