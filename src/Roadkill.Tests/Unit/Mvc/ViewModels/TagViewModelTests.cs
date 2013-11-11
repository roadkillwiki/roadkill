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

		[Test]
		public void Equals_Should_Use_Name_Property_For_Equality()
		{
			// Arrange
			TagViewModel model1 = new TagViewModel("tag1");
			TagViewModel model2 = new TagViewModel("tag2");

			// Act
			bool areEqual = model1.Equals(model2);

			// Assert
			Assert.That(areEqual, Is.False);
		}

		[Test]
		public void Hashcode_Should_Change_For_Different_Objects_And_Not_Change_For_Lifetime_Of_Object()
		{
			// Arrange
			TagViewModel model1 = new TagViewModel("tag1");
			TagViewModel model2 = new TagViewModel("tag2");

			// Act
			int hashCode1 = model1.GetHashCode();
			int hashCode2 = model2.GetHashCode();
			int hashCode1a = model1.GetHashCode();


			// Assert
			Assert.That(hashCode1, Is.Not.EqualTo(hashCode2));
			Assert.That(hashCode1, Is.EqualTo(hashCode1a));
		}

		[Test]
		public void ToString_Should_Return_Name()
		{
			// Arrange
			TagViewModel model1 = new TagViewModel("tag1");

			// Act + Assert
			Assert.That(model1.ToString(), Is.EqualTo("tag1"));
		}
	}
}
