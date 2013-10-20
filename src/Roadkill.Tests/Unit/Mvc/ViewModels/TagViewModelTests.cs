using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Plugins;
using Roadkill.Tests.Unit.StubsAndMocks;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class TagViewModelTests
	{
		[Test]
		public void Constructor_Should_Fill_Properties()
		{
			// Arrange + Act
			TagViewModel model = new TagViewModel("tag1");	

			// Assert
			Assert.That(model.Name, Is.EqualTo("tag1"));
			Assert.That(model.Count, Is.EqualTo(1));
		}
	}
}
