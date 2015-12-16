using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	[TestFixture]
	[Category("Unit")]
	public class TestResultTests
	{
		[Test]
		public void constructor_should_set_error_message_property()
		{
			// Act + Arrange
			TestResult result = new TestResult("some error");

			// Assert
			Assert.That(result.ErrorMessage, Is.EqualTo("some error"));
		}

		[Test]
		public void success_should_be_true_when_error_message_is_empty()
		{
			// Act + Arrange
			TestResult result = new TestResult("");

			// Assert
			Assert.That(result.Success, Is.True);
		}
	}
}
