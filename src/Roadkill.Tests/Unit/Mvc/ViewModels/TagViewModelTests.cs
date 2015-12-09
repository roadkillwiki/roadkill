using NUnit.Framework;
using Roadkill.Core.Mvc.ViewModels;

namespace Roadkill.Tests.Unit.Mvc.ViewModels
{
	[TestFixture]
	[Category("Unit")]
	public class TagViewModelTests
	{
		[Test]
		public void constructor_should_fill_properties()
		{
			// Arrange + Act
			TagViewModel model = new TagViewModel("tag1");	

			// Assert
			Assert.That(model.Name, Is.EqualTo("tag1"));
			Assert.That(model.Count, Is.EqualTo(1));
		}

		[Test]
		public void equals_should_use_name_property_for_equality()
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
		public void hashcode_should_change_for_different_objects_and_not_change_for_lifetime_of_object()
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
		public void tostring_should_return_name()
		{
			// Arrange
			TagViewModel model1 = new TagViewModel("tag1");

			// Act + Assert
			Assert.That(model1.ToString(), Is.EqualTo("tag1"));
		}
	}
}
