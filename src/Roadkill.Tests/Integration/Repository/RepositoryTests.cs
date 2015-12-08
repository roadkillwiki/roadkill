using System;
using System.Threading;
using NUnit.Framework;
using Roadkill.Core.Configuration;
using Roadkill.Core.Database;

namespace Roadkill.Tests.Integration.Repository
{
	[TestFixture]
	[Category("Integration")]
	public abstract class RepositoryTests
	{
		protected IRepository Repository;
		protected ApplicationSettings ApplicationSettings;

		protected abstract string ConnectionString { get; }

		[SetUp]
		public void SetUp()
		{
			Clearup();
			ApplicationSettings = new ApplicationSettings()
			{
				ConnectionString = ConnectionString, 
				DatabaseName = "SqlServer2008",
				LoggingTypes =  "none"
			};

			Repository = GetRepository();
		}

		[TearDown]
		public void TearDown()
		{
			Repository.Dispose();
		}

		protected abstract IRepository GetRepository();
		protected abstract void Clearup();
	}
}
