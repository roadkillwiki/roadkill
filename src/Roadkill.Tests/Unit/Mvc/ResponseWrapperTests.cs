using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core.Attachments;

namespace Roadkill.Tests.Unit.Mvc
{
	[TestFixture]
	[Category("Unit")]
	public class ResponseWrapperTests
	{
		[Test]
		public void GetStatusCodeForCache_Should_Return_200_When_File_Was_Written_More_Recently()
		{
			// Arrange
			DateTime fileLastWritten = DateTime.Today;
			string ifModifiedSince = DateTime.Today.AddDays(-1).ToString("r"); // last time it was checked

			// Act
			int status = ResponseWrapper.GetStatusCodeForCache(fileLastWritten, ifModifiedSince);

			// Assert
			Assert.That(status, Is.EqualTo(200));
		}

		[Test]
		public void GetStatusCodeForCache_Should_Return_304_When_File_Was_Checked_More_Recently()
		{
			// Arrange
			DateTime fileLastWritten = DateTime.Today.AddDays(-1);
			string ifModifiedSince = fileLastWritten.ToString("r"); // last modified stores the modified/write time of the file, i.e. it's exact

			// Act
			int status = ResponseWrapper.GetStatusCodeForCache(fileLastWritten, ifModifiedSince);

			// Assert
			Assert.That(status, Is.EqualTo(304));
		}
	}
}
