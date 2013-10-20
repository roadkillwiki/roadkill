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
	public class PageViewModelTests
	{
		[Test]
		public void Constructor_Should_Fill_Properties()
		{
			// Arrange

			// Act
			PageViewModel model = new PageViewModel();			

			// Assert
			
		}

		// Content
		// IsNew
		// RawTags

		// CommaDelimitedTags
		// SpaceDelimitedTags
		// ParseRawTags
		// ParseTags
		// VerifyRawTags
	}
}
