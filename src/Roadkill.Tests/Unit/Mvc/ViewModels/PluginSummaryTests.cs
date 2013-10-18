using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PluginSummaryTests
	{
		[Test]
		public void Constructor_Should_Create_SettingValues()
		{
			// Arrange + Act
			PluginSummary summary = new PluginSummary();	

			// Assert
			Assert.That(summary.SettingValues, Is.Not.Null);
		}
	}
}
