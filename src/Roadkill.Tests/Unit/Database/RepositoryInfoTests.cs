using NUnit.Framework;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Unit.Database
{
	public class RepositoryInfoTests
	{
		[Test]
		public void should_set_properties()
		{
			// Arrange
			var info = new RepositoryInfo("SqlDB", "A Sql Server Mongo Redis Document Store");

			// Act + Assert
			Assert.That(info.Id, Is.EqualTo("SqlDB"));
			Assert.That(info.Description, Is.EqualTo("A Sql Server Mongo Redis Document Store"));
		}

		[Test]
		public void Id_should_compare_with_string_and_be_case_insensitive()
		{
			// Arrange
			var info = new RepositoryInfo("SqlDB", "A Sql Server Mongo Redis Document Store");
			
			// Act + Assert
			Assert.True(info == "sqldb");
			Assert.True("sqldb" == info);
		}

		[Test]
		public void Equals_should_be_case_insensitive()
		{
			// Arrange
			var info = new RepositoryInfo("SqlDB", "A Sql Server Mongo Redis Document Store");

			// Act + Assert
			Assert.True(info.Equals(new RepositoryInfo("sqldb", "notused")));
		}
	}
}
