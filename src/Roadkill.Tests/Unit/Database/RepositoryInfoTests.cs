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
		public void id_should_compare_with_string_and_be_case_insensitive()
		{
			// Arrange
			var info = new RepositoryInfo("SqlDB", "A Sql Server Mongo Redis Document Store");

			// Act + Assert
			Assert.True(info == "sqldb");
			Assert.True("sqldb" == info);

			Assert.True(info != "foo");
			Assert.True("foo" != info);
		}

		[Test]
		public void id_should_return_false_when_compared_with_null_and_empty_strings()
		{
			// Arrange
			var info = new RepositoryInfo("", "");
			RepositoryInfo nullInfo = null;

			// Act + Assert
			Assert.True(nullInfo == null);

			Assert.False(info == null);
			Assert.False(null == info);
			Assert.False(info == "");
			Assert.False("" == info);

			Assert.True(info != null);
			Assert.True(null != info);		
			Assert.True(info != "");
			Assert.True("" != info);
		}

		[Test]
		public void equals_should_be_case_insensitive()
		{
			// Arrange
			var info = new RepositoryInfo("SqlDB", "A Sql Server Mongo Redis Document Store");

			// Act + Assert
			Assert.True(info.Equals(new RepositoryInfo("sqldb", "notused")));
		}
	}
}
