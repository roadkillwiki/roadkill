using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Roadkill.Core;
using Roadkill.Core.Database;
using Roadkill.Core.Database.MongoDB;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class DataStoreTypeTests
	{
		[Test]
		public void ShouldContainTypesAndBeEnumerable()
		{
			// Arrange
			IEnumerable<DataStoreType> dataStoreTypes = DataStoreType.AllTypes;

			// Act + Assert
			foreach (DataStoreType type in dataStoreTypes)
			{
				Assert.That(type.Name, Is.Not.Empty);
				Assert.That(type.Description, Is.Not.Empty);
				Console.WriteLine("Found {0}", type);
			}
		}

		[Test]
		public void ShouldContainMongoDB()
		{
			// Arrange
			IEnumerable<DataStoreType> dataStoreTypes = DataStoreType.AllTypes;

			// Act
			DataStoreType mongoType = DataStoreType.AllTypes.First(x => x.Name == "MongoDB");
			
			// Assert
			Assert.That(mongoType.RequiresCustomRepository, Is.True);
			Assert.That(mongoType.CustomRepositoryType, Contains.Substring(typeof(MongoDBRepository).FullName));
		}

		[Test]
		public void ByNameShouldFindTypeAndBeCaseInsensitive()
		{
			// Arrange
			IEnumerable<DataStoreType> dataStoreTypes = DataStoreType.AllTypes;

			// Act
			DataStoreType datastoreType = DataStoreType.ByName("mOngoDB");

			// Assert
			Assert.That(datastoreType.Name, Is.EqualTo("MongoDB"));
		}
	}
}
