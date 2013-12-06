using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Moq;
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
		public void GetStatusCodeForCache_Should_Return_200_When_No_Modified_Since_Header()
		{
			// Arrange
			DateTime fileLastWritten = DateTime.Today;
			string ifModifiedSince = null;

			// Act
			int status = ResponseWrapper.GetStatusCodeForCache(fileLastWritten, ifModifiedSince);

			// Assert
			Assert.That(status, Is.EqualTo(200));
		}

		[Test]
		public void GetStatusCodeForCache_Should_Return_304_When_LastModified_Date_Matches_File_Last_Write_Date()
		{
			// Arrange
			DateTime fileLastWritten = DateTime.Today.AddDays(-1);
			string ifModifiedSince = fileLastWritten.ToString("r"); // last modified stores the modified/write time of the file, i.e. it's exact

			// Act
			int status = ResponseWrapper.GetStatusCodeForCache(fileLastWritten, ifModifiedSince);

			// Assert
			Assert.That(status, Is.EqualTo(304));
		}

		[Test]
		public void BinaryWrite_Should_Add_Content_Type()
		{
			// Arrange
			Mock<HttpResponseBase> responseMock = new Mock<HttpResponseBase>();
			responseMock.SetupAllProperties();
			
			HttpResponseBase response = responseMock.Object;
			ResponseWrapper wrapper = new ResponseWrapper(response);
			wrapper.ContentType = "image/jpeg";

			// Act
			wrapper.BinaryWrite(new byte[] { });

			// Assert
			Assert.That(response.ContentType, Is.EqualTo("image/jpeg"));
		}

		
		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase("gibberish")]
		public void GetLastModifiedDate_Should_Return_DateTimeMin_For_Empty_Last_Modified_Dates(string lastModifiedHeader)
		{
			// Arrange

			// Act
			DateTime modifiedDate = ResponseWrapper.GetLastModifiedDate(lastModifiedHeader);

			// Assert
			Assert.That(modifiedDate, Is.EqualTo(DateTime.MinValue));
		}

		[Test]
		public void GetLastModifiedDate_Should_Return_Valid_DateTime_For_Known_Date_With_No_Milliseconds()
		{
			// Arrange
			string lastModifiedHeader = "Thu, 07 Nov 2013 12:32:40 GMT"; // direct from Firebug
			DateTime expectedDateTime = new DateTime(2013, 11, 07, 12, 32, 40);

			// Act
			DateTime modifiedDate = ResponseWrapper.GetLastModifiedDate(lastModifiedHeader);

			// Assert
			Assert.That(modifiedDate, Is.EqualTo(expectedDateTime));
			Assert.That(modifiedDate.Millisecond, Is.EqualTo(0));
		}
	}
}
