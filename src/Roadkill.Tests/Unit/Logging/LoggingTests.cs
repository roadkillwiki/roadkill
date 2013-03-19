using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Common;
namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class Log4jXmlParseTests
	{
		[Test]
		public void Should_Deserialize_With_No_Xml_Declaration_And_Events_Item()
		{
			// Arrange

			// Act
			List<Log4jEvent> eventList = LogReader.Load(@".\Unit\Logging\default.log").ToList();

			// Assert
			Assert.That(eventList.Count, Is.EqualTo(14));
		}

		[Test]
		public void Should_Deserialize_WITH_Xml_Declaration_And_Events_Item()
		{
			// Arrange

			// Act
			List<Log4jEvent> eventList = LogReader.Load(@".\Unit\Logging\xmldeclaration.log").ToList();

			// Assert
			Assert.That(eventList.Count, Is.EqualTo(14));
		}


		[Test]
		public void Should_Deserialize_Single_Event()
		{
			// Arrange
			string text = File.ReadAllText(@".\Unit\Logging\singlevent.log");		

			// Act
			Log4jEvent logEvent = Log4jEvent.DeserializeElement(text);

			// Assert
			Assert.That(logEvent, Is.Not.Null);
			Assert.That(logEvent.Id, Is.Not.EqualTo(Guid.Empty));
			Assert.That(logEvent.Level, Is.EqualTo("ERROR"));
			Assert.That(logEvent.Message, Is.StringContaining("Error caught on logviewer.Index:"));
			Assert.That(logEvent.Logger, Is.EqualTo("Log4jXmlWriter"));
			Assert.That(logEvent.Timestamp, Is.EqualTo(DateTime.Parse("2013/03/19 19:36:03.155")));
			Assert.That(logEvent.Properties.Count, Is.EqualTo(2));
			Assert.That(logEvent.Properties[0].Value, Is.EqualTo("CORSAIR"));
			Assert.That(logEvent.Properties[1].Value, Is.EqualTo("Roadkill.Core, Version=1.6.1.0, Culture=neutral, PublicKeyToken=null"));
		}
	}
}