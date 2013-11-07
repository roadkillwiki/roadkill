using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Roadkill.Core.Database;
using Roadkill.Core.Database.Export;

namespace Roadkill.Tests.Unit
{
	[TestFixture]
	[Category("Unit")]
	public class SqlExportBuilderTests
	{
		[Test]
		public void Should_Export_Users_With_All_FieldValues()
		{
			// Arrange
			Guid user1Id = new Guid("29a8ad19-b203-46f5-be10-11e0ebf6f812");
			Guid user2Id = new Guid("e63b0023-329a-49b9-97a4-5094a0e378a2");
			Guid user3Id = new Guid("a6ee19ef-c093-47de-97d2-83dec406d92d");

			Guid user1Activationkey = new Guid("0953cf95-f357-4e5b-ae2b-7541844d3b6b");
			Guid user2Activationkey = new Guid("aa87fe31-9781-4c93-b7e3-9092ed095810");
			Guid user3Activationkey = new Guid("b8ef994d-87f5-4543-85de-66b41244a20a");

			User user1 = new User()
			{
				Id = user1Id,
				ActivationKey = user1Activationkey.ToString(),
				Firstname = "firstname1",
				Lastname = "lastname1",
				Email = "user1@localhost", 
				Password = "encrypted1",
				Salt = "salt1",
				IsActivated = true,
				IsAdmin = true,
				Username = "user1"
			};

			User user2 = new User()
			{
				Id = user2Id,
				ActivationKey = user2Activationkey.ToString(),
				Firstname = "firstname2",
				Lastname = "lastname2",
				Email = "user2@localhost",
				Password = "encrypted2",
				Salt = "salt2",
				IsActivated = true,
				IsEditor = true,
				Username = "user2"
			};

			User user3 = new User()
			{
				Id = user3Id,
				ActivationKey = user3Activationkey.ToString(),
				Firstname = "firstname3",
				Lastname = "lastname3",
				Email = "user3@localhost",
				Password = "encrypted3",
				Salt = "salt3",
				IsActivated = false,
				IsEditor = true,
				Username = "user3"
			};


			RepositoryMock repository = new RepositoryMock();
			repository.Users.Add(user1);
			repository.Users.Add(user2);
			repository.Users.Add(user3);

			SqlExportBuilder builder = new SqlExportBuilder(repository);
			string expectedSql = ReadEmbeddedResource("expected-users-export.sql");

			// Act
			string actualSql = builder.Export();

			// Assert
			Assert.That(actualSql, Is.EqualTo(expectedSql), actualSql);
		}

		private string ReadEmbeddedResource(string name)
		{
			string path = string.Format("Roadkill.Tests.Unit.Database.Export.{0}", name);

			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
			if (stream == null)
				throw new InvalidOperationException(string.Format("Unable to find '{0}' as an embedded resource", path));

			string result = "";
			using (StreamReader reader = new StreamReader(stream))
			{
				result = reader.ReadToEnd();
			}

			return result;
		}
	}
}
