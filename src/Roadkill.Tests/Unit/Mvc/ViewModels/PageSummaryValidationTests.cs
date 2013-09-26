using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;
using Roadkill.Core.Mvc.ViewModels;
using Roadkill.Core.Security;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class PageSummaryValidationTests
	{
		[Test]
		public void VerifyRawTags_With_Ok_Characters_Should_Succeed()
		{
			// Arrange
			PageSummary summary = new PageSummary();
			summary.RawTags = "tagone, #anothertag, tag-2, code, c++";

			// Act
			ValidationResult result = PageSummary.VerifyRawTags(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyRawTags_With_Empty_String_Should_Succeed()
		{
			// Arrange
			PageSummary summary = new PageSummary();
			summary.RawTags = "";

			// Act
			ValidationResult result = PageSummary.VerifyRawTags(summary, null);

			// Assert
			Assert.That(result, Is.EqualTo(ValidationResult.Success));
		}

		[Test]
		public void VerifyRawTags_With_Bad_Characters_Should_Fail()
		{
			// Arrange
			PageSummary summary = new PageSummary();
			summary.RawTags = "&&amp+some,tags,only,^^,!??malicious,monkey,would,use";

			// Act
			ValidationResult result = PageSummary.VerifyRawTags(summary, null);

			// Assert
			Assert.That(result, Is.Not.EqualTo(ValidationResult.Success));
		}
	}
}
