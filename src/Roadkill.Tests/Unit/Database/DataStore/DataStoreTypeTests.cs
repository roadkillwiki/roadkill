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
		public void AllTypes_Should_Contain_Types_With_Name_And_Description()
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
		public void AllTypes_Should_Contain_MongoDB_With_Custom_Repository()
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
		public void ByName_Should_Find_DataStoreType_And_Be_Case_Insensitive()
		{
			// Arrange
			IEnumerable<DataStoreType> dataStoreTypes = DataStoreType.AllTypes;

			// Act
			DataStoreType datastoreType = DataStoreType.ByName("mOngoDB");

			// Assert
			Assert.That(datastoreType.Name, Is.EqualTo("MongoDB"));
		}

		[Test]
		[ExpectedException(typeof(DatabaseException))]
		public void ByName_Should_Throw_DatabaseException_If_DataStoreType_Not_Found()
		{
			// Arrange
			IEnumerable<DataStoreType> dataStoreTypes = DataStoreType.AllTypes;

			// Act + Assert
			DataStoreType datastoreType = DataStoreType.ByName("thunderbird");
		}

		[Test]
		public void AllMonoTypes_Should_Only_Contain_Dbs_Supported_On_Linux()
		{
			// Arrange

			// Act
			IEnumerable<DataStoreType> dataStoreTypes = DataStoreType.AllMonoTypes;

			// Assert
			Assert.That(dataStoreTypes.Count(), Is.EqualTo(3));
			Assert.That(dataStoreTypes, Contains.Item(DataStoreType.MongoDB));
			Assert.That(dataStoreTypes, Contains.Item(DataStoreType.MySQL));
			Assert.That(dataStoreTypes, Contains.Item(DataStoreType.Postgres));
		}
	}
}
