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
		public void Constructor_Should_Set_Error_Message_Property()
		{
			// Act + Arrange
			TestResult result = new TestResult("some error");

			// Assert
			Assert.That(result.ErrorMessage, Is.EqualTo("some error"));
		}

		[Test]
		public void Success_Should_Be_True_When_Error_Message_Is_Empty()
		{
			// Act + Arrange
			TestResult result = new TestResult("");

			// Assert
			Assert.That(result.Success, Is.True);
		}
	}
}
